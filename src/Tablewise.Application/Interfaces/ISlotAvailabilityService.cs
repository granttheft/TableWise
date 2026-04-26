namespace Tablewise.Application.Interfaces;

/// <summary>
/// Slot müsaitlik servisi.
/// Mekanın çalışma saatleri, kapalılıklar ve mevcut rezervasyonlara göre
/// müsait slotları hesaplar.
/// </summary>
public interface ISlotAvailabilityService
{
    /// <summary>
    /// Belirtilen gün için müsait slotları getirir.
    /// </summary>
    /// <param name="venueId">Mekan ID</param>
    /// <param name="date">Tarih</param>
    /// <param name="partySize">Kişi sayısı</param>
    /// <param name="tableId">Belirli bir masa için kontrol (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Müsait slot listesi</returns>
    Task<IReadOnlyList<AvailableSlot>> GetAvailableSlotsAsync(
        Guid venueId,
        DateTime date,
        int partySize,
        Guid? tableId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir slot'un müsait olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="venueId">Mekan ID</param>
    /// <param name="startTime">Başlangıç zamanı (UTC)</param>
    /// <param name="endTime">Bitiş zamanı (UTC)</param>
    /// <param name="partySize">Kişi sayısı</param>
    /// <param name="tableId">Belirli bir masa (opsiyonel)</param>
    /// <param name="excludeReservationId">Hariç tutulacak rezervasyon ID (değişiklik için)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Müsait ise true</returns>
    Task<SlotAvailabilityResult> CheckSlotAvailabilityAsync(
        Guid venueId,
        DateTime startTime,
        DateTime endTime,
        int partySize,
        Guid? tableId = null,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mekan için availability cache'ini invalidate eder.
    /// Rezervasyon oluşturma/iptal/güncelleme sonrası çağrılmalı.
    /// </summary>
    /// <param name="venueId">Mekan ID</param>
    /// <param name="date">Tarih</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task InvalidateCacheAsync(
        Guid venueId,
        DateTime date,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Müsait slot bilgisi.
/// </summary>
public sealed record AvailableSlot
{
    /// <summary>
    /// Slot başlangıç zamanı (UTC).
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Slot bitiş zamanı (UTC).
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Müsait masalar.
    /// </summary>
    public IReadOnlyList<AvailableTable> AvailableTables { get; init; } = [];

    /// <summary>
    /// Müsait masa birleşimleri.
    /// </summary>
    public IReadOnlyList<AvailableTableCombination> AvailableCombinations { get; init; } = [];

    /// <summary>
    /// Toplam müsait kapasite.
    /// </summary>
    public int TotalCapacity { get; init; }
}

/// <summary>
/// Müsait masa bilgisi.
/// </summary>
public sealed record AvailableTable
{
    /// <summary>
    /// Masa ID.
    /// </summary>
    public Guid TableId { get; init; }

    /// <summary>
    /// Masa adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Masa kapasitesi.
    /// </summary>
    public int Capacity { get; init; }

    /// <summary>
    /// Masa lokasyonu.
    /// </summary>
    public string Location { get; init; } = string.Empty;
}

/// <summary>
/// Müsait masa birleşimi bilgisi.
/// </summary>
public sealed record AvailableTableCombination
{
    /// <summary>
    /// Birleşim ID.
    /// </summary>
    public Guid CombinationId { get; init; }

    /// <summary>
    /// Birleşim adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Toplam kapasite.
    /// </summary>
    public int CombinedCapacity { get; init; }

    /// <summary>
    /// Birleştirilen masa ID'leri.
    /// </summary>
    public IReadOnlyList<Guid> TableIds { get; init; } = [];
}

/// <summary>
/// Slot müsaitlik kontrol sonucu.
/// </summary>
public sealed record SlotAvailabilityResult
{
    /// <summary>
    /// Slot müsait mi?
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Müsait değilse neden.
    /// </summary>
    public string? UnavailabilityReason { get; init; }

    /// <summary>
    /// Önerilen masa ID (opsiyonel).
    /// </summary>
    public Guid? SuggestedTableId { get; init; }

    /// <summary>
    /// Önerilen masa birleşimi ID (opsiyonel).
    /// </summary>
    public Guid? SuggestedCombinationId { get; init; }

    /// <summary>
    /// Müsait sonucu oluşturur.
    /// </summary>
    public static SlotAvailabilityResult Available(Guid? suggestedTableId = null, Guid? suggestedCombinationId = null)
        => new()
        {
            IsAvailable = true,
            SuggestedTableId = suggestedTableId,
            SuggestedCombinationId = suggestedCombinationId
        };

    /// <summary>
    /// Müsait değil sonucu oluşturur.
    /// </summary>
    public static SlotAvailabilityResult Unavailable(string reason)
        => new()
        {
            IsAvailable = false,
            UnavailabilityReason = reason
        };
}
