using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed class GetPricingPlansQueryHandler : IRequestHandler<GetPricingPlansQuery, IReadOnlyList<PlanPricingDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPricingPlansQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<PlanPricingDto>> Handle(GetPricingPlansQuery request, CancellationToken cancellationToken)
    {
        return await _db.Plans
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Tier)
            .Select(p => new PlanPricingDto(p.Id, p.Name, p.Tier.ToString(), p.MonthlyPriceTry, p.YearlyPriceTry, p.IsVisible))
            .ToListAsync(cancellationToken);
    }
}
