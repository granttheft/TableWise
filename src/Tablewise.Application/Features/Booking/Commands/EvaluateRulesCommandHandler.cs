using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Booking;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Booking.Commands;

/// <summary>
/// EvaluateRulesCommand handler.
/// </summary>
public sealed class EvaluateRulesCommandHandler : IRequestHandler<EvaluateRulesCommand, EvaluateRulesResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRuleEvaluator _ruleEvaluator;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public EvaluateRulesCommandHandler(
        IUnitOfWork unitOfWork,
        IRuleEvaluator ruleEvaluator)
    {
        _unitOfWork = unitOfWork;
        _ruleEvaluator = ruleEvaluator;
    }

    /// <inheritdoc />
    public async Task<EvaluateRulesResponseDto> Handle(EvaluateRulesCommand request, CancellationToken cancellationToken)
    {
        // Venue bul
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

        // Mevcut müşteri varsa tier bilgisini al
        string? customerTier = null;
        if (!string.IsNullOrEmpty(request.CustomerPhone))
        {
            var customer = await _unitOfWork.Customers
                .Query()
                .IgnoreQueryFilters()
                .Where(c => c.TenantId == venue.TenantId &&
                           c.Phone == request.CustomerPhone &&
                           !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            customerTier = customer?.Tier.ToString();
        }

        // Kural değerlendir
        var context = new RuleEvaluationContext
        {
            VenueId = venue.Id,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            CustomerTier = customerTier,
            ReservedFor = request.ReservedFor,
            PartySize = request.PartySize,
            TableId = request.TableId,
            Source = "BookingUI"
        };

        var result = await _ruleEvaluator.EvaluateAsync(context, cancellationToken)
            .ConfigureAwait(false);

        // Deposit hesapla
        decimal? depositAmount = null;
        if (result.RequiresDeposit || venue.DepositEnabled)
        {
            depositAmount = result.DepositAmount ??
                (venue.DepositPerPerson
                    ? venue.DepositAmount * request.PartySize
                    : venue.DepositAmount);
        }

        return new EvaluateRulesResponseDto
        {
            IsAllowed = result.IsAllowed,
            BlockReason = result.BlockReason,
            DiscountPercent = result.DiscountPercent,
            RequiresDeposit = result.RequiresDeposit || venue.DepositEnabled,
            DepositAmount = depositAmount,
            Warnings = result.Warnings.ToList()
        };
    }
}
