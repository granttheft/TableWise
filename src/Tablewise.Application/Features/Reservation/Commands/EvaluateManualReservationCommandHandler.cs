using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.Common;
using Tablewise.Application.DTOs.Reservation;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// EvaluateManualReservationCommand handler.
/// </summary>
public sealed class EvaluateManualReservationCommandHandler
    : IRequestHandler<EvaluateManualReservationCommand, EvaluateManualReservationResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ISlotAvailabilityService _slotService;
    private readonly IRuleEvaluator _ruleEvaluator;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public EvaluateManualReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ISlotAvailabilityService slotService,
        IRuleEvaluator ruleEvaluator)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _slotService = slotService;
        _ruleEvaluator = ruleEvaluator;
    }

    /// <inheritdoc />
    public async Task<EvaluateManualReservationResponseDto> Handle(
        EvaluateManualReservationCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var reservedFor = DateTimeNormalization.ToUtcReservedFor(request.ReservedFor);

        var venue = await _unitOfWork.Venues
            .Query()
            .Where(v => v.Id == request.VenueId && v.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId.ToString(), "Mekan bulunamadı.");
        }

        var blockers = new List<ReservationEvaluationItemDto>();
        var warnings = new List<ReservationEvaluationItemDto>();

        var slotEndTime = reservedFor.AddMinutes(venue.SlotDurationMinutes);
        var availability = await _slotService.CheckSlotAvailabilityAsync(
                venue.Id,
                reservedFor,
                slotEndTime,
                request.PartySize,
                request.TableId,
                null,
                cancellationToken)
            .ConfigureAwait(false);

        Guid? effectiveTableId = request.TableId ?? availability.SuggestedTableId;

        if (!availability.IsAvailable)
        {
            blockers.Add(new ReservationEvaluationItemDto
            {
                Message = availability.UnavailabilityReason ?? "Seçilen slot müsait değil.",
                RuleId = string.Empty,
            });
        }

        string? customerTier = null;
        string? customerEmail = request.GuestEmail;
        string? customerPhone = request.GuestPhone;

        if (request.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers
                .Query()
                .Where(c => c.Id == request.CustomerId.Value && c.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (customer != null)
            {
                customerTier = customer.Tier.ToString();
                customerEmail ??= customer.Email;
                customerPhone ??= customer.Phone;
            }
        }
        else if (!string.IsNullOrEmpty(request.GuestPhone))
        {
            var customer = await _unitOfWork.Customers
                .Query()
                .Where(c => c.TenantId == tenantId && c.Phone == request.GuestPhone && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            customerTier = customer?.Tier.ToString();
        }

        var ruleContext = new RuleEvaluationContext
        {
            VenueId = venue.Id,
            CustomerEmail = customerEmail,
            CustomerPhone = customerPhone,
            CustomerTier = customerTier,
            ReservedFor = reservedFor,
            PartySize = request.PartySize,
            TableId = effectiveTableId,
            Source = "ManualAdmin",
        };

        var ruleResult = await _ruleEvaluator.EvaluateAsync(ruleContext, cancellationToken)
            .ConfigureAwait(false);

        foreach (var warning in ruleResult.Warnings)
        {
            warnings.Add(new ReservationEvaluationItemDto
            {
                Message = warning,
                RuleId = string.Empty,
            });
        }

        if (!ruleResult.IsAllowed && !string.IsNullOrWhiteSpace(ruleResult.BlockReason))
        {
            blockers.Add(new ReservationEvaluationItemDto
            {
                Message = ruleResult.BlockReason,
                RuleId = string.Empty,
            });
        }

        decimal? depositAmount = null;
        var depositRequired = ruleResult.RequiresDeposit || venue.DepositEnabled;
        if (depositRequired)
        {
            depositAmount = ruleResult.DepositAmount ??
                            (venue.DepositPerPerson
                                ? venue.DepositAmount * request.PartySize
                                : venue.DepositAmount);
        }

        var isAllowed = blockers.Count == 0 && ruleResult.IsAllowed;

        return new EvaluateManualReservationResponseDto
        {
            IsAllowed = isAllowed,
            Warnings = warnings,
            Blockers = blockers,
            DiscountPercent = ruleResult.DiscountPercent,
            DepositRequired = depositRequired,
            DepositAmount = depositAmount,
            AppliedRules = ruleResult.AppliedRules.Select(r => r.RuleName).ToList(),
        };
    }
}
