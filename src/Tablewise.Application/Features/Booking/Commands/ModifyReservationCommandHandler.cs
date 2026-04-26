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
/// ModifyReservationCommand handler.
/// Mevcut rezervasyonu Modified durumuna geçirir ve yeni rezervasyon oluşturur.
/// </summary>
public sealed class ModifyReservationCommandHandler : IRequestHandler<ModifyReservationCommand, ReserveResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlotAvailabilityService _slotService;
    private readonly IRuleEvaluator _ruleEvaluator;
    private readonly IDistributedLockService _lockService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ModifyReservationCommandHandler> _logger;

    private const int ModificationDeadlineHours = 24;
    private const int ConfirmCodeLength = 8;
    private const int MaxConfirmCodeRetries = 3;
    private const string ConfirmCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LockWaitTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public ModifyReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ISlotAvailabilityService slotService,
        IRuleEvaluator ruleEvaluator,
        IDistributedLockService lockService,
        ICacheService cacheService,
        IEmailService emailService,
        ILogger<ModifyReservationCommandHandler> logger)
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
    public async Task<ReserveResponseDto> Handle(ModifyReservationCommand request, CancellationToken cancellationToken)
    {
        // 1. Mevcut rezervasyonu bul
        var existingReservation = await _unitOfWork.Reservations
            .Query()
            .IgnoreQueryFilters()
            .Include(r => r.Venue)
            .Where(r => r.ConfirmCode == request.ConfirmCode && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingReservation == null)
        {
            throw new NotFoundException("Reservation", request.ConfirmCode, "Rezervasyon bulunamadı.");
        }

        // 2. Durum kontrolü
        if (existingReservation.Status != ReservationStatus.Confirmed)
        {
            throw new BusinessRuleException("Sadece onaylanmış rezervasyonlar değiştirilebilir.");
        }

        // 3. 24 saat kontrolü
        var hoursUntilReservation = (existingReservation.ReservedFor - DateTime.UtcNow).TotalHours;
        if (hoursUntilReservation < ModificationDeadlineHours)
        {
            throw new BusinessRuleException(
                $"Rezervasyon değişikliği en az {ModificationDeadlineHours} saat öncesinden yapılmalıdır. " +
                $"Kalan süre: {hoursUntilReservation:F1} saat.");
        }

        var venue = existingReservation.Venue!;
        var tenantId = existingReservation.TenantId;

        // 4. Yeni değerleri belirle
        var newDateTime = request.NewDateTime ?? existingReservation.ReservedFor;
        var newPartySize = request.NewPartySize ?? existingReservation.PartySize;
        var newTableId = request.NewTableId ?? existingReservation.TableId;
        var newEndTime = newDateTime.AddMinutes(venue.SlotDurationMinutes);

        // En az bir değişiklik olmalı
        if (newDateTime == existingReservation.ReservedFor &&
            newPartySize == existingReservation.PartySize &&
            newTableId == existingReservation.TableId)
        {
            throw new BusinessRuleException("En az bir değişiklik yapılmalıdır.");
        }

        // 5. Distributed Lock al
        var lockKey = $"reserve:{venue.Id}:{newDateTime:yyyyMMddHHmm}";
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
            // 6. Yeni slot müsaitlik kontrolü (mevcut rezervasyonu hariç tut)
            var availability = await _slotService.CheckSlotAvailabilityAsync(
                venue.Id,
                newDateTime,
                newEndTime,
                newPartySize,
                newTableId,
                existingReservation.Id, // Mevcut rezervasyonu hariç tut
                cancellationToken)
                .ConfigureAwait(false);

            if (!availability.IsAvailable)
            {
                throw new ConflictException(availability.UnavailabilityReason ?? "Seçilen slot müsait değil.");
            }

            // 7. Kural motoru değerlendir
            var ruleContext = new RuleEvaluationContext
            {
                VenueId = venue.Id,
                CustomerId = existingReservation.CustomerId,
                CustomerEmail = existingReservation.GuestEmail,
                CustomerPhone = existingReservation.GuestPhone,
                ReservedFor = newDateTime,
                PartySize = newPartySize,
                TableId = newTableId ?? availability.SuggestedTableId,
                TableCombinationId = existingReservation.TableCombinationId ?? availability.SuggestedCombinationId,
                Source = "BookingUI"
            };

            var ruleResult = await _ruleEvaluator.EvaluateAsync(ruleContext, cancellationToken)
                .ConfigureAwait(false);

            if (!ruleResult.IsAllowed)
            {
                throw new BusinessRuleException(ruleResult.BlockReason ?? "Kural ihlali nedeniyle değişiklik reddedildi.");
            }

            // 8. Eski rezervasyonu Modified durumuna geçir
            var oldStatus = existingReservation.Status;
            existingReservation.Status = ReservationStatus.Modified;

            var oldStatusLog = new ReservationStatusLog
            {
                ReservationId = existingReservation.Id,
                FromStatus = oldStatus,
                ToStatus = ReservationStatus.Modified,
                ChangedBy = "Customer",
                Reason = "Rezervasyon değiştirildi"
            };
            await _unitOfWork.ReservationStatusLogs.AddAsync(oldStatusLog, cancellationToken).ConfigureAwait(false);

            // 9. Yeni ConfirmCode üret
            var newConfirmCode = await GenerateUniqueConfirmCodeAsync(cancellationToken).ConfigureAwait(false);

            // 10. Yeni rezervasyon oluştur
            var newReservation = new Domain.Entities.Reservation
            {
                TenantId = tenantId,
                VenueId = venue.Id,
                TableId = newTableId ?? availability.SuggestedTableId,
                TableCombinationId = existingReservation.TableCombinationId ?? availability.SuggestedCombinationId,
                CustomerId = existingReservation.CustomerId,
                GuestName = existingReservation.GuestName,
                GuestEmail = existingReservation.GuestEmail,
                GuestPhone = existingReservation.GuestPhone,
                PartySize = newPartySize,
                ReservedFor = newDateTime,
                EndTime = newEndTime,
                Status = ReservationStatus.Confirmed,
                Source = existingReservation.Source,
                ConfirmCode = newConfirmCode,
                SpecialRequests = existingReservation.SpecialRequests,
                DiscountPercent = ruleResult.DiscountPercent,
                AppliedRulesSnapshot = SerializeAppliedRules(ruleResult.AppliedRules),
                CustomFieldAnswers = existingReservation.CustomFieldAnswers,
                DepositStatus = existingReservation.DepositStatus,
                DepositAmount = existingReservation.DepositAmount,
                DepositPaymentRef = existingReservation.DepositPaymentRef,
                DepositPaidAt = existingReservation.DepositPaidAt,
                ModifiedFromReservationId = existingReservation.Id
            };

            await _unitOfWork.Reservations.AddAsync(newReservation, cancellationToken).ConfigureAwait(false);

            // 11. Yeni status log
            var newStatusLog = new ReservationStatusLog
            {
                ReservationId = newReservation.Id,
                FromStatus = ReservationStatus.Pending,
                ToStatus = ReservationStatus.Confirmed,
                ChangedBy = "System",
                Reason = "Değişiklikten oluşturuldu"
            };
            await _unitOfWork.ReservationStatusLogs.AddAsync(newStatusLog, cancellationToken).ConfigureAwait(false);

            // 12. Audit log
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                EntityType = "Reservation",
                EntityId = newReservation.Id.ToString(),
                Action = "Modified",
                PerformedBy = "Customer",
                NewValue = $"OldCode: {existingReservation.ConfirmCode}, NewCode: {newConfirmCode}",
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // 13. Cache invalidate (her iki tarih için)
            await _slotService.InvalidateCacheAsync(venue.Id, existingReservation.ReservedFor.Date, cancellationToken)
                .ConfigureAwait(false);
            await _slotService.InvalidateCacheAsync(venue.Id, newDateTime.Date, cancellationToken)
                .ConfigureAwait(false);

            // 14. Email gönder
            _ = SendModificationEmailAsync(newReservation, venue.Name, existingReservation);

            _logger.LogInformation(
                "Rezervasyon değiştirildi. Eski: {OldCode}, Yeni: {NewCode}",
                existingReservation.ConfirmCode, newConfirmCode);

            return new ReserveResponseDto
            {
                ReservationId = newReservation.Id,
                ConfirmCode = newConfirmCode,
                Status = newReservation.Status.ToString(),
                ReservedFor = newReservation.ReservedFor,
                EndTime = newReservation.EndTime,
                VenueName = venue.Name,
                TableName = await GetTableNameAsync(newReservation.TableId, newReservation.TableCombinationId, cancellationToken),
                PartySize = newReservation.PartySize,
                DepositRequired = false,
                DepositAmount = newReservation.DepositAmount,
                DiscountPercent = ruleResult.DiscountPercent,
                Warnings = ruleResult.Warnings.ToList()
            };
        }
        finally
        {
            // Lock otomatik dispose
        }
    }

    #region Private Helpers

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

            if (!exists) return code;
        }

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

    private static string? SerializeAppliedRules(IReadOnlyList<AppliedRuleSnapshot> rules)
    {
        if (rules.Count == 0) return null;
        return JsonSerializer.Serialize(rules, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private async Task<string?> GetTableNameAsync(Guid? tableId, Guid? combinationId, CancellationToken cancellationToken)
    {
        if (tableId.HasValue)
        {
            return await _unitOfWork.Tables.Query().IgnoreQueryFilters()
                .Where(t => t.Id == tableId.Value).Select(t => t.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        if (combinationId.HasValue)
        {
            return await _unitOfWork.TableCombinations.Query().IgnoreQueryFilters()
                .Where(c => c.Id == combinationId.Value).Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        return null;
    }

    private async Task SendModificationEmailAsync(Domain.Entities.Reservation newReservation, string venueName, Domain.Entities.Reservation oldReservation)
    {
        if (string.IsNullOrEmpty(newReservation.GuestEmail)) return;

        try
        {
            await _emailService.SendReservationModificationAsync(
                newReservation.GuestEmail,
                newReservation.GuestName,
                venueName,
                oldReservation.ReservedFor,
                newReservation.ReservedFor,
                newReservation.ConfirmCode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Değişiklik email'i gönderilemedi. ReservationId: {ReservationId}", newReservation.Id);
        }
    }

    #endregion
}
