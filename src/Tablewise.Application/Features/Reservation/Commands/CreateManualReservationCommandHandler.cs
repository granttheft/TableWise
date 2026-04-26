using System.Security.Cryptography;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.DTOs.Reservation;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// CreateManualReservationCommand handler.
/// </summary>
public sealed class CreateManualReservationCommandHandler : IRequestHandler<CreateManualReservationCommand, ReservationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ISlotAvailabilityService _slotService;
    private readonly IRuleEvaluator _ruleEvaluator;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateManualReservationCommandHandler> _logger;

    private const int ConfirmCodeLength = 8;
    private const string ConfirmCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public CreateManualReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ISlotAvailabilityService slotService,
        IRuleEvaluator ruleEvaluator,
        IEmailService emailService,
        ILogger<CreateManualReservationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _slotService = slotService;
        _ruleEvaluator = ruleEvaluator;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReservationDto> Handle(CreateManualReservationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // 1. Venue kontrolü
        var venue = await _unitOfWork.Venues
            .Query()
            .Where(v => v.Id == request.VenueId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId.ToString(), "Mekan bulunamadı.");
        }

        var slotEndTime = request.ReservedFor.AddMinutes(venue.SlotDurationMinutes);

        // 2. Slot müsaitlik kontrolü
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

        // 3. Kural değerlendirme (bypass değilse)
        RuleEvaluationResult? ruleResult = null;
        if (!request.BypassRules)
        {
            var ruleContext = new RuleEvaluationContext
            {
                VenueId = venue.Id,
                CustomerEmail = request.GuestEmail,
                CustomerPhone = request.GuestPhone,
                ReservedFor = request.ReservedFor,
                PartySize = request.PartySize,
                TableId = request.TableId ?? availability.SuggestedTableId,
                Source = "ManualAdmin"
            };

            ruleResult = await _ruleEvaluator.EvaluateAsync(ruleContext, cancellationToken)
                .ConfigureAwait(false);

            if (!ruleResult.IsAllowed)
            {
                throw new BusinessRuleException(ruleResult.BlockReason ?? "Kural ihlali nedeniyle rezervasyon reddedildi.");
            }
        }

        // 4. Customer bul/oluştur
        var customer = await FindOrCreateCustomerAsync(
            tenantId,
            request.GuestName,
            request.GuestPhone,
            request.GuestEmail,
            cancellationToken)
            .ConfigureAwait(false);

        // 5. Kapora durumu
        var depositRequired = !request.BypassDeposit && venue.DepositEnabled;
        var depositStatus = depositRequired ? DepositStatus.Pending : DepositStatus.NotRequired;

        // 6. ConfirmCode üret
        var confirmCode = await GenerateUniqueConfirmCodeAsync(cancellationToken).ConfigureAwait(false);

        // 7. Rezervasyon oluştur
        var source = _currentUser.Role == UserRole.Owner
            ? ReservationSource.ManualAdmin
            : ReservationSource.ManualStaff;

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
            Status = ReservationStatus.Confirmed,
            Source = source,
            ConfirmCode = confirmCode,
            SpecialRequests = request.SpecialRequests,
            InternalNotes = request.InternalNotes,
            DiscountPercent = ruleResult?.DiscountPercent,
            AppliedRulesSnapshot = ruleResult != null ? SerializeAppliedRules(ruleResult.AppliedRules) : null,
            DepositStatus = depositStatus,
            DepositAmount = depositRequired ? venue.DepositAmount : null
        };

        _unitOfWork.Reservations.Add(reservation);

        // 8. Status log
        var statusLog = new ReservationStatusLog
        {
            ReservationId = reservation.Id,
            FromStatus = ReservationStatus.Pending,
            ToStatus = ReservationStatus.Confirmed,
            ChangedByUserId = _currentUser.UserId,
            ChangedBy = _currentUser.Email
        };
        _unitOfWork.ReservationStatusLogs.Add(statusLog);

        // 9. Audit log
        var auditLog = new AuditLog
        {
            TenantId = tenantId,
            EntityType = "Reservation",
            EntityId = reservation.Id,
            Action = "ManualCreated",
            PerformedBy = _currentUser.Email ?? "Staff",
            Details = $"Manuel rezervasyon oluşturuldu. ConfirmCode: {confirmCode}"
        };
        _unitOfWork.AuditLogs.Add(auditLog);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 10. Cache invalidate
        await _slotService.InvalidateCacheAsync(venue.Id, request.ReservedFor.Date, cancellationToken)
            .ConfigureAwait(false);

        // 11. Email gönder
        if (request.SendConfirmationEmail && !string.IsNullOrEmpty(request.GuestEmail))
        {
            _ = SendConfirmationEmailAsync(reservation, venue.Name);
        }

        _logger.LogInformation(
            "Manuel rezervasyon oluşturuldu. Id: {ReservationId}, By: {User}",
            reservation.Id, _currentUser.Email);

        // 12. Response döndür
        return new ReservationDto
        {
            Id = reservation.Id,
            VenueId = reservation.VenueId,
            VenueName = venue.Name,
            TableId = reservation.TableId,
            TableName = await GetTableNameAsync(reservation.TableId, cancellationToken),
            CustomerId = reservation.CustomerId,
            GuestName = reservation.GuestName,
            GuestEmail = reservation.GuestEmail,
            GuestPhone = reservation.GuestPhone,
            PartySize = reservation.PartySize,
            ReservedFor = reservation.ReservedFor,
            EndTime = reservation.EndTime,
            Status = reservation.Status.ToString(),
            Source = reservation.Source.ToString(),
            ConfirmCode = reservation.ConfirmCode,
            SpecialRequests = reservation.SpecialRequests,
            InternalNotes = reservation.InternalNotes,
            DepositStatus = reservation.DepositStatus.ToString(),
            DepositAmount = reservation.DepositAmount,
            CreatedAt = reservation.CreatedAt
        };
    }

    #region Private Helpers

    private async Task<Customer?> FindOrCreateCustomerAsync(
        Guid tenantId,
        string name,
        string phone,
        string? email,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Customers
            .Query()
            .Where(c => c.Phone == phone)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existing != null)
        {
            if (!string.IsNullOrEmpty(email) && string.IsNullOrEmpty(existing.Email))
                existing.Email = email;
            existing.LastReservationAt = DateTime.UtcNow;
            return existing;
        }

        var newCustomer = new Customer
        {
            TenantId = tenantId,
            FullName = name,
            Phone = phone,
            Email = email,
            Tier = CustomerTier.Regular,
            LastReservationAt = DateTime.UtcNow
        };
        _unitOfWork.Customers.Add(newCustomer);
        return newCustomer;
    }

    private async Task<string> GenerateUniqueConfirmCodeAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < 3; i++)
        {
            var code = GenerateConfirmCode();
            var exists = await _unitOfWork.Reservations.Query()
                .AnyAsync(r => r.ConfirmCode == code, cancellationToken)
                .ConfigureAwait(false);
            if (!exists) return code;
        }
        return GenerateConfirmCode() + DateTime.UtcNow.Ticks.ToString("X")[..2];
    }

    private static string GenerateConfirmCode()
    {
        var bytes = new byte[ConfirmCodeLength];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[ConfirmCodeLength];
        for (var i = 0; i < ConfirmCodeLength; i++)
            chars[i] = ConfirmCodeChars[bytes[i] % ConfirmCodeChars.Length];
        return new string(chars);
    }

    private static string? SerializeAppliedRules(IReadOnlyList<AppliedRuleSnapshot> rules)
    {
        if (rules.Count == 0) return null;
        return JsonSerializer.Serialize(rules, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private async Task<string?> GetTableNameAsync(Guid? tableId, CancellationToken cancellationToken)
    {
        if (!tableId.HasValue) return null;
        return await _unitOfWork.Tables.Query()
            .Where(t => t.Id == tableId.Value)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task SendConfirmationEmailAsync(Domain.Entities.Reservation reservation, string venueName)
    {
        try
        {
            await _emailService.SendReservationConfirmationAsync(
                reservation.GuestEmail!,
                reservation.GuestName,
                venueName,
                reservation.ReservedFor,
                reservation.ConfirmCode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email gönderilemedi. ReservationId: {Id}", reservation.Id);
        }
    }

    #endregion
}
