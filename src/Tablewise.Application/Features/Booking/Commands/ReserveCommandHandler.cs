using System.Security.Cryptography;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.DTOs.Booking;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Booking.Commands;

/// <summary>
/// ReserveCommand handler.
/// Race condition önleme için distributed lock kullanır.
/// </summary>
public sealed class ReserveCommandHandler : IRequestHandler<ReserveCommand, ReserveResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlotAvailabilityService _slotService;
    private readonly IRuleEvaluator _ruleEvaluator;
    private readonly IDistributedLockService _lockService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReserveCommandHandler> _logger;

    private const int ConfirmCodeLength = 8;
    private const int MaxConfirmCodeRetries = 3;
    private const string ConfirmCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Karışıklık önlemek için 0,1,I,O hariç
    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LockWaitTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public ReserveCommandHandler(
        IUnitOfWork unitOfWork,
        ISlotAvailabilityService slotService,
        IRuleEvaluator ruleEvaluator,
        IDistributedLockService lockService,
        ICacheService cacheService,
        IEmailService emailService,
        ILogger<ReserveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _slotService = slotService;
        _ruleEvaluator = ruleEvaluator;
        _lockService = lockService;
        _cacheService = cacheService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReserveResponseDto> Handle(ReserveCommand request, CancellationToken cancellationToken)
    {
        // 1. Venue bul (slug ile, tenant filter bypass)
        var venue = await _unitOfWork.Venues
            .Query()
            .IgnoreQueryFilters()
            .Include(v => v.Tenant)
            .Where(v => v.Tenant != null &&
                        v.Tenant.Slug == request.Slug &&
                        !v.Tenant.IsDeleted &&
                        v.Tenant.IsActive &&
                        !v.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.Slug, "Mekan bulunamadı.");
        }

        var tenantId = venue.TenantId;
        var slotEndTime = request.ReservedFor.AddMinutes(venue.SlotDurationMinutes);

        // 2. Distributed Lock al (race condition önleme)
        var lockKey = $"reserve:{venue.Id}:{request.ReservedFor:yyyyMMddHHmm}";
        await using var lockHandle = await _lockService.WaitForLockAsync(
            lockKey,
            LockExpiry,
            LockWaitTimeout,
            cancellationToken)
            .ConfigureAwait(false);

        if (lockHandle == null)
        {
            throw new ConflictException("Bu slot için eş zamanlı işlem yapılıyor. Lütfen tekrar deneyin.");
        }

        try
        {
            // 3. Slot müsaitlik kontrolü (lock altında)
            var availability = await _slotService.CheckSlotAvailabilityAsync(
                venue.Id,
                request.ReservedFor,
                slotEndTime,
                request.PartySize,
                request.TableId,
                null,
                cancellationToken)
                .ConfigureAwait(false);

            if (!availability.IsAvailable)
            {
                throw new ConflictException(availability.UnavailabilityReason ?? "Seçilen slot müsait değil.");
            }

            // 4. Kural motoru değerlendir (Faz 3'te gerçek implementasyon)
            var ruleContext = new RuleEvaluationContext
            {
                VenueId = venue.Id,
                CustomerEmail = request.GuestEmail,
                CustomerPhone = request.GuestPhone,
                ReservedFor = request.ReservedFor,
                PartySize = request.PartySize,
                TableId = request.TableId ?? availability.SuggestedTableId,
                TableCombinationId = request.TableCombinationId ?? availability.SuggestedCombinationId,
                CustomFieldAnswers = request.CustomFieldAnswers,
                Source = "BookingUI"
            };

            var ruleResult = await _ruleEvaluator.EvaluateAsync(ruleContext, cancellationToken)
                .ConfigureAwait(false);

            if (!ruleResult.IsAllowed)
            {
                throw new BusinessRuleException(ruleResult.BlockReason ?? "Kural ihlali nedeniyle rezervasyon reddedildi.");
            }

            // 5. Customer bul veya oluştur
            var customer = await FindOrCreateCustomerAsync(
                tenantId,
                request.GuestName,
                request.GuestPhone,
                request.GuestEmail,
                cancellationToken)
                .ConfigureAwait(false);

            // 6. Kapora durumunu belirle
            var depositRequired = venue.DepositEnabled && (ruleResult.RequiresDeposit || venue.DepositAmount > 0);
            var depositAmount = ruleResult.DepositAmount ?? CalculateDepositAmount(venue, request.PartySize);
            var depositStatus = depositRequired ? DepositStatus.Pending : DepositStatus.NotRequired;
            var reservationStatus = depositRequired ? ReservationStatus.Pending : ReservationStatus.Confirmed;

            // 7. ConfirmCode üret (benzersiz)
            var confirmCode = await GenerateUniqueConfirmCodeAsync(cancellationToken).ConfigureAwait(false);

            // 8. Rezervasyon oluştur
            var reservation = new Domain.Entities.Reservation
            {
                TenantId = tenantId,
                VenueId = venue.Id,
                TableId = request.TableId ?? availability.SuggestedTableId,
                TableCombinationId = request.TableCombinationId ?? availability.SuggestedCombinationId,
                CustomerId = customer?.Id,
                GuestName = request.GuestName,
                GuestEmail = request.GuestEmail,
                GuestPhone = request.GuestPhone,
                PartySize = request.PartySize,
                ReservedFor = request.ReservedFor,
                EndTime = slotEndTime,
                Status = reservationStatus,
                Source = ReservationSource.BookingUI,
                ConfirmCode = confirmCode,
                SpecialRequests = request.SpecialRequests,
                DiscountPercent = ruleResult.DiscountPercent,
                AppliedRulesSnapshot = SerializeAppliedRules(ruleResult.AppliedRules),
                CustomFieldAnswers = SerializeCustomFieldAnswers(request.CustomFieldAnswers),
                DepositStatus = depositStatus,
                DepositAmount = depositRequired ? depositAmount : null
            };

            await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken).ConfigureAwait(false);

            // 9. Status log
            var statusLog = new ReservationStatusLog
            {
                ReservationId = reservation.Id,
                FromStatus = ReservationStatus.Pending,
                ToStatus = reservationStatus,
                ChangedBy = "System"
            };
            await _unitOfWork.ReservationStatusLogs.AddAsync(statusLog, cancellationToken).ConfigureAwait(false);

            // 10. Audit log
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                EntityType = "Reservation",
                EntityId = reservation.Id.ToString(),
                Action = "Created",
                PerformedBy = "BookingUI",
                NewValue = $"ConfirmCode: {confirmCode}, GuestName: {request.GuestName}",
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken).ConfigureAwait(false);

            // 11. Save
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Cache invalidate
            await _slotService.InvalidateCacheAsync(venue.Id, request.ReservedFor.Date, cancellationToken)
                .ConfigureAwait(false);

            // 12. Reservation count artır (Redis atomic)
            var countKey = $"tenant:{tenantId}:reservations:{DateTime.UtcNow:yyyyMM}";
            await _cacheService.IncrementAsync(countKey, TimeSpan.FromDays(35), cancellationToken)
                .ConfigureAwait(false);

            // 13. Email kuyruğa at (async fire-and-forget)
            _ = SendConfirmationEmailAsync(reservation, venue.Name);

            _logger.LogInformation(
                "Rezervasyon oluşturuldu. Id: {ReservationId}, ConfirmCode: {ConfirmCode}, Venue: {VenueName}",
                reservation.Id, confirmCode, venue.Name);

            // 14. Response döndür
            return new ReserveResponseDto
            {
                ReservationId = reservation.Id,
                ConfirmCode = confirmCode,
                Status = reservationStatus.ToString(),
                ReservedFor = reservation.ReservedFor,
                EndTime = reservation.EndTime,
                VenueName = venue.Name,
                TableName = await GetTableNameAsync(reservation.TableId, reservation.TableCombinationId, cancellationToken),
                PartySize = reservation.PartySize,
                DepositRequired = depositRequired,
                DepositAmount = depositRequired ? depositAmount : null,
                PaymentUrl = depositRequired ? GeneratePaymentUrl(reservation.Id) : null,
                DiscountPercent = ruleResult.DiscountPercent,
                Warnings = ruleResult.Warnings.ToList()
            };
        }
        finally
        {
            // Lock otomatik olarak dispose edilecek
        }
    }

    #region Private Helpers

    private async Task<Customer?> FindOrCreateCustomerAsync(
        Guid tenantId,
        string name,
        string phone,
        string? email,
        CancellationToken cancellationToken)
    {
        // Telefon ile mevcut müşteri ara
        var existingCustomer = await _unitOfWork.Customers
            .Query()
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == tenantId && c.Phone == phone && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingCustomer != null)
        {
            // Bilgileri güncelle
            if (!string.IsNullOrEmpty(email) && string.IsNullOrEmpty(existingCustomer.Email))
            {
                existingCustomer.Email = email;
            }
            if (existingCustomer.FullName != name)
            {
                existingCustomer.FullName = name;
            }
            existingCustomer.LastReservationAt = DateTime.UtcNow;

            return existingCustomer;
        }

        // Yeni müşteri oluştur
        var newCustomer = new Customer
        {
            TenantId = tenantId,
            FullName = name,
            Phone = phone,
            Email = email,
            Tier = CustomerTier.Regular,
            LastReservationAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(newCustomer, cancellationToken).ConfigureAwait(false);

        return newCustomer;
    }

    private async Task<string> GenerateUniqueConfirmCodeAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < MaxConfirmCodeRetries; i++)
        {
            var code = GenerateConfirmCode();

            var exists = await _unitOfWork.Reservations
                .Query()
                .IgnoreQueryFilters()
                .AnyAsync(r => r.ConfirmCode == code, cancellationToken)
                .ConfigureAwait(false);

            if (!exists)
            {
                return code;
            }

            _logger.LogWarning("ConfirmCode çakışması: {Code}, retry: {Retry}", code, i + 1);
        }

        // Son çare: timestamp ekle
        return GenerateConfirmCode() + DateTime.UtcNow.Ticks.ToString("X").Substring(0, 2);
    }

    private static string GenerateConfirmCode()
    {
        var bytes = new byte[ConfirmCodeLength];
        RandomNumberGenerator.Fill(bytes);

        var chars = new char[ConfirmCodeLength];
        for (var i = 0; i < ConfirmCodeLength; i++)
        {
            chars[i] = ConfirmCodeChars[bytes[i] % ConfirmCodeChars.Length];
        }

        return new string(chars);
    }

    private static decimal CalculateDepositAmount(Domain.Entities.Venue venue, int partySize)
    {
        if (!venue.DepositEnabled || !venue.DepositAmount.HasValue)
            return 0;

        return venue.DepositPerPerson
            ? venue.DepositAmount.Value * partySize
            : venue.DepositAmount.Value;
    }

    private static string? SerializeAppliedRules(IReadOnlyList<AppliedRuleSnapshot> rules)
    {
        if (rules.Count == 0)
            return null;

        return JsonSerializer.Serialize(rules, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static string? SerializeCustomFieldAnswers(Dictionary<string, string>? answers)
    {
        if (answers == null || answers.Count == 0)
            return null;

        return JsonSerializer.Serialize(answers);
    }

    private async Task<string?> GetTableNameAsync(Guid? tableId, Guid? combinationId, CancellationToken cancellationToken)
    {
        if (tableId.HasValue)
        {
            var table = await _unitOfWork.Tables
                .Query()
                .IgnoreQueryFilters()
                .Where(t => t.Id == tableId.Value)
                .Select(t => t.Name)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return table;
        }

        if (combinationId.HasValue)
        {
            var combo = await _unitOfWork.TableCombinations
                .Query()
                .IgnoreQueryFilters()
                .Where(c => c.Id == combinationId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return combo;
        }

        return null;
    }

    private static string? GeneratePaymentUrl(Guid reservationId)
    {
        // Faz 7'de İyzico entegrasyonu ile gerçek URL üretilecek
        return $"/payment/{reservationId}";
    }

    private async Task SendConfirmationEmailAsync(Domain.Entities.Reservation reservation, string venueName)
    {
        if (string.IsNullOrEmpty(reservation.GuestEmail))
            return;

        try
        {
            await _emailService.SendReservationConfirmationAsync(
                reservation.GuestEmail,
                reservation.GuestName,
                venueName,
                reservation.ReservedFor,
                reservation.ConfirmCode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Onay email'i gönderilemedi. ReservationId: {ReservationId}", reservation.Id);
        }
    }

    #endregion
}
