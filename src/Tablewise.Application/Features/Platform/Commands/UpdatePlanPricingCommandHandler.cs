using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed class UpdatePlanPricingCommandHandler : IRequestHandler<UpdatePlanPricingCommand, PlanPricingDto>
{
    private readonly IApplicationDbContext _db;

    public UpdatePlanPricingCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlanPricingDto> Handle(UpdatePlanPricingCommand request, CancellationToken cancellationToken)
    {
        var plan = await _db.Plans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Plan", request.PlanId);

        plan.MonthlyPriceTry = request.Dto.MonthlyPriceTry;
        plan.YearlyPriceTry = request.Dto.YearlyPriceTry;

        if (request.Dto.LimitsJson is not null)
        {
            try { System.Text.Json.JsonDocument.Parse(request.Dto.LimitsJson); }
            catch { throw new ArgumentException("LimitsJson geçerli bir JSON değil."); }
            plan.LimitsJson = request.Dto.LimitsJson;
        }

        if (request.Dto.FeaturesJson is not null)
        {
            try { System.Text.Json.JsonDocument.Parse(request.Dto.FeaturesJson); }
            catch { throw new ArgumentException("FeaturesJson geçerli bir JSON değil."); }
            plan.FeaturesJson = request.Dto.FeaturesJson;
        }

        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return new PlanPricingDto(plan.Id, plan.Name, plan.Tier.ToString(), plan.MonthlyPriceTry, plan.YearlyPriceTry, plan.IsVisible, plan.LimitsJson, plan.FeaturesJson);
    }
}
