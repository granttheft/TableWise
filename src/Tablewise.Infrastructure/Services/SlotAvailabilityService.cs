using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Slot müsaitlik servisi.
/// Çalışma saatleri, kapalılıklar ve mevcut rezervasyonları kontrol ederek müsait slotları hesaplar.
/// </summary>
public sealed class SlotAvailabilityService : ISlotAvailabilityService
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SlotAvailabilityService> _logger;

    private const int CacheTtlMinutes = 5;
    private const string CacheKeyPrefix = "avail:";

    /// <summary>
    /// SlotAvailabilityService constructor.
    /// </summary>
    public SlotAvailabilityService(
        TablewiseDbContext dbContext,
        ICacheService cacheService,
        ILogger<SlotAvailabilityService> logger)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AvailableSlot>> GetAvailableSlotsAsync(
        Guid venueId,
        DateTime date,
        int partySize,
        Guid? tableId = null,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        var cacheKey = BuildCacheKey(venueId, dateOnly);

        // Cache kontrolü (tableId ve partySize olmadan genel veri)
        // Daha sonra partySize'a göre filter edilir

        // Venue bilgilerini al
        var venue = await _dbContext.Venues
            .AsNoTracking()
            .Where(v => v.Id == venueId && !v.IsDeleted)
            .Select(v => new
            {
                v.Id,
                v.SlotDurationMinutes,
                v.WorkingHours,
                v.OpeningTime,
                v.ClosingTime,
                v.TimeZone
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            _logger.LogWarning("Venue bulunamadı: {VenueId}", venueId);
            return [];
        }

        // Kapalılık kontrolü
        var closure = await _dbContext.VenueClosures
            .AsNoTracking()
            .Where(c => c.VenueId == venueId && c.Date.Date == dateOnly && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (closure?.IsFullDay == true)
        {
            _logger.LogDebug("Mekan tam gün kapalı: {VenueId}, {Date}", venueId, dateOnly);
            return [];
        }

        // Çalışma saatlerini belirle
        var (openTime, closeTime) = GetWorkingHours(venue.WorkingHours, venue.OpeningTime, venue.ClosingTime, dateOnly);

        if (openTime == null || closeTime == null)
        {
            _logger.LogDebug("Mekan çalışma saatleri belirsiz: {VenueId}, {Date}", venueId, dateOnly);
            return [];
        }

        // Kısmi kapalılık varsa saatleri override et
        if (closure != null && !closure.IsFullDay)
        {
            if (closure.OpenTime.HasValue)
                openTime = closure.OpenTime.Value;
            if (closure.CloseTime.HasValue)
                closeTime = closure.CloseTime.Value;
        }

        // Aktif masaları al
        var tables = await _dbContext.Tables
            .AsNoTracking()
            .Where(t => t.VenueId == venueId && t.IsActive && !t.IsDeleted)
            .Where(t => tableId == null || t.Id == tableId)
            .Select(t => new TableInfo
            {
                Id = t.Id,
                Name = t.Name,
                Capacity = t.Capacity,
                Location = t.Location.ToString()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Masa birleşimlerini al
        var combinations = await _dbContext.TableCombinations
            .AsNoTracking()
            .Where(tc => tc.VenueId == venueId && tc.IsActive && !tc.IsDeleted)
            .Select(tc => new CombinationInfo
            {
                Id = tc.Id,
                Name = tc.Name,
                CombinedCapacity = tc.CombinedCapacity,
                TableIdsJson = tc.TableIds
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Mevcut rezervasyonları al (Pending veya Confirmed)
        var reservations = await _dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.VenueId == venueId &&
                        r.ReservedFor.Date == dateOnly &&
                        !r.IsDeleted &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
            .Select(r => new ReservationInfo
            {
                TableId = r.TableId,
                TableCombinationId = r.TableCombinationId,
                StartTime = r.ReservedFor,
                EndTime = r.EndTime
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Combination table ID'lerini parse et
        foreach (var combo in combinations)
        {
            combo.TableIds = ParseTableIds(combo.TableIdsJson);
        }

        // Slotları hesapla
        var slots = new List<AvailableSlot>();
        var currentTime = dateOnly.Add(openTime.Value);
        var endOfDay = dateOnly.Add(closeTime.Value);
        var slotDuration = TimeSpan.FromMinutes(venue.SlotDurationMinutes);

        while (currentTime.Add(slotDuration) <= endOfDay)
        {
            var slotStart = currentTime;
            var slotEnd = currentTime.Add(slotDuration);

            // Bu slot için müsait masaları bul
            var availableTables = new List<AvailableTable>();
            var availableCombinations = new List<AvailableTableCombination>();

            foreach (var table in tables.Where(t => t.Capacity >= partySize))
            {
                if (!IsTableOccupied(table.Id, slotStart, slotEnd, reservations, combinations))
                {
                    availableTables.Add(new AvailableTable
                    {
                        TableId = table.Id,
                        Name = table.Name,
                        Capacity = table.Capacity,
                        Location = table.Location
                    });
                }
            }

            foreach (var combo in combinations.Where(c => c.CombinedCapacity >= partySize))
            {
                if (!IsCombinationOccupied(combo, slotStart, slotEnd, reservations))
                {
                    availableCombinations.Add(new AvailableTableCombination
                    {
                        CombinationId = combo.Id,
                        Name = combo.Name,
                        CombinedCapacity = combo.CombinedCapacity,
                        TableIds = combo.TableIds
                    });
                }
            }

            if (availableTables.Count > 0 || availableCombinations.Count > 0)
            {
                slots.Add(new AvailableSlot
                {
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    AvailableTables = availableTables,
                    AvailableCombinations = availableCombinations,
                    TotalCapacity = availableTables.Sum(t => t.Capacity) + availableCombinations.Sum(c => c.CombinedCapacity)
                });
            }

            currentTime = currentTime.Add(slotDuration);
        }

        return slots;
    }

    /// <inheritdoc />
    public async Task<SlotAvailabilityResult> CheckSlotAvailabilityAsync(
        Guid venueId,
        DateTime startTime,
        DateTime endTime,
        int partySize,
        Guid? tableId = null,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = startTime.Date;

        // Venue kontrolü
        var venue = await _dbContext.Venues
            .AsNoTracking()
            .Where(v => v.Id == venueId && !v.IsDeleted)
            .Select(v => new { v.Id, v.WorkingHours, v.OpeningTime, v.ClosingTime })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            return SlotAvailabilityResult.Unavailable("Mekan bulunamadı.");
        }

        // Kapalılık kontrolü
        var closure = await _dbContext.VenueClosures
            .AsNoTracking()
            .Where(c => c.VenueId == venueId && c.Date.Date == dateOnly && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (closure?.IsFullDay == true)
        {
            return SlotAvailabilityResult.Unavailable("Mekan bu tarihte kapalı.");
        }

        // Çalışma saatleri kontrolü
        var (openTime, closeTime) = GetWorkingHours(venue.WorkingHours, venue.OpeningTime, venue.ClosingTime, dateOnly);

        if (openTime == null || closeTime == null)
        {
            return SlotAvailabilityResult.Unavailable("Mekan bu gün kapalı.");
        }

        var slotTimeOfDay = startTime.TimeOfDay;
        var endTimeOfDay = endTime.TimeOfDay;

        if (slotTimeOfDay < openTime.Value || endTimeOfDay > closeTime.Value)
        {
            return SlotAvailabilityResult.Unavailable("Seçilen saat çalışma saatleri dışında.");
        }

        // Belirli masa istendi mi?
        if (tableId.HasValue)
        {
            var table = await _dbContext.Tables
                .AsNoTracking()
                .Where(t => t.Id == tableId.Value && t.VenueId == venueId && t.IsActive && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (table == null)
            {
                return SlotAvailabilityResult.Unavailable("Seçilen masa mevcut değil.");
            }

            if (table.Capacity < partySize)
            {
                return SlotAvailabilityResult.Unavailable($"Seçilen masanın kapasitesi ({table.Capacity} kişi) yetersiz.");
            }

            // Masa müsait mi kontrol et
            var conflicting = await _dbContext.Reservations
                .AsNoTracking()
                .Where(r => r.VenueId == venueId &&
                            r.TableId == tableId.Value &&
                            !r.IsDeleted &&
                            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                            (excludeReservationId == null || r.Id != excludeReservationId.Value) &&
                            r.ReservedFor < endTime &&
                            r.EndTime > startTime)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (conflicting)
            {
                return SlotAvailabilityResult.Unavailable("Seçilen masa bu saatte dolu.");
            }

            return SlotAvailabilityResult.Available(tableId);
        }

        // Masa istenmedi, uygun masa bul
        var availableTables = await _dbContext.Tables
            .AsNoTracking()
            .Where(t => t.VenueId == venueId && t.IsActive && !t.IsDeleted && t.Capacity >= partySize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var occupiedTableIds = await _dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.VenueId == venueId &&
                        r.TableId != null &&
                        !r.IsDeleted &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                        (excludeReservationId == null || r.Id != excludeReservationId.Value) &&
                        r.ReservedFor < endTime &&
                        r.EndTime > startTime)
            .Select(r => r.TableId!.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var freeTable = availableTables.FirstOrDefault(t => !occupiedTableIds.Contains(t.Id));

        if (freeTable != null)
        {
            return SlotAvailabilityResult.Available(freeTable.Id);
        }

        // Masa birleşimlerine bak
        var combinations = await _dbContext.TableCombinations
            .AsNoTracking()
            .Where(tc => tc.VenueId == venueId && tc.IsActive && !tc.IsDeleted && tc.CombinedCapacity >= partySize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var combo in combinations)
        {
            var comboTableIds = ParseTableIds(combo.TableIds);
            var anyOccupied = comboTableIds.Any(id => occupiedTableIds.Contains(id));

            // Komboya ait rezervasyon var mı?
            var comboOccupied = await _dbContext.Reservations
                .AsNoTracking()
                .Where(r => r.VenueId == venueId &&
                            r.TableCombinationId == combo.Id &&
                            !r.IsDeleted &&
                            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                            (excludeReservationId == null || r.Id != excludeReservationId.Value) &&
                            r.ReservedFor < endTime &&
                            r.EndTime > startTime)
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!anyOccupied && !comboOccupied)
            {
                return SlotAvailabilityResult.Available(suggestedCombinationId: combo.Id);
            }
        }

        return SlotAvailabilityResult.Unavailable("Bu saat için uygun masa bulunamadı.");
    }

    /// <inheritdoc />
    public async Task InvalidateCacheAsync(
        Guid venueId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(venueId, date.Date);

        try
        {
            await _cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Availability cache invalidated: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidation başarısız: {CacheKey}", cacheKey);
        }
    }

    #region Private Helpers

    private static string BuildCacheKey(Guid venueId, DateTime date)
    {
        return $"{CacheKeyPrefix}{venueId}:{date:yyyy-MM-dd}";
    }

    private static (TimeSpan? Open, TimeSpan? Close) GetWorkingHours(
        string? workingHoursJson,
        TimeSpan defaultOpen,
        TimeSpan defaultClose,
        DateTime date)
    {
        if (string.IsNullOrEmpty(workingHoursJson))
        {
            return (defaultOpen, defaultClose);
        }

        try
        {
            var workingHours = JsonSerializer.Deserialize<Dictionary<string, WorkingHourEntry>>(
                workingHoursJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (workingHours == null)
                return (defaultOpen, defaultClose);

            var dayName = date.DayOfWeek.ToString();

            if (workingHours.TryGetValue(dayName, out var entry))
            {
                if (entry.IsClosed)
                    return (null, null);

                var open = TimeSpan.TryParse(entry.Open, out var o) ? o : defaultOpen;
                var close = TimeSpan.TryParse(entry.Close, out var c) ? c : defaultClose;
                return (open, close);
            }

            return (defaultOpen, defaultClose);
        }
        catch
        {
            return (defaultOpen, defaultClose);
        }
    }

    private static List<Guid> ParseTableIds(string tableIdsJson)
    {
        if (string.IsNullOrEmpty(tableIdsJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(tableIdsJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static bool IsTableOccupied(
        Guid tableId,
        DateTime slotStart,
        DateTime slotEnd,
        List<ReservationInfo> reservations,
        List<CombinationInfo> combinations)
    {
        // Direkt masa rezervasyonu var mı?
        if (reservations.Any(r => r.TableId == tableId && r.StartTime < slotEnd && r.EndTime > slotStart))
            return true;

        // Masayı içeren bir kombinasyon rezervasyonu var mı?
        var comboIds = combinations.Where(c => c.TableIds.Contains(tableId)).Select(c => c.Id).ToList();

        return reservations.Any(r =>
            r.TableCombinationId != null &&
            comboIds.Contains(r.TableCombinationId.Value) &&
            r.StartTime < slotEnd &&
            r.EndTime > slotStart);
    }

    private static bool IsCombinationOccupied(
        CombinationInfo combo,
        DateTime slotStart,
        DateTime slotEnd,
        List<ReservationInfo> reservations)
    {
        // Kombinasyon direkt rezerve edilmiş mi?
        if (reservations.Any(r =>
            r.TableCombinationId == combo.Id &&
            r.StartTime < slotEnd &&
            r.EndTime > slotStart))
            return true;

        // Kombinasyondaki masalardan biri ayrı rezerve edilmiş mi?
        return reservations.Any(r =>
            r.TableId.HasValue &&
            combo.TableIds.Contains(r.TableId.Value) &&
            r.StartTime < slotEnd &&
            r.EndTime > slotStart);
    }

    #endregion

    #region Helper Classes

    private sealed class WorkingHourEntry
    {
        public string Open { get; init; } = string.Empty;
        public string Close { get; init; } = string.Empty;
        public bool IsClosed { get; init; }
    }

    private sealed class TableInfo
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Capacity { get; init; }
        public string Location { get; init; } = string.Empty;
    }

    private sealed class CombinationInfo
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int CombinedCapacity { get; init; }
        public string TableIdsJson { get; init; } = "[]";
        public List<Guid> TableIds { get; set; } = [];
    }

    private sealed class ReservationInfo
    {
        public Guid? TableId { get; init; }
        public Guid? TableCombinationId { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
    }

    #endregion
}
