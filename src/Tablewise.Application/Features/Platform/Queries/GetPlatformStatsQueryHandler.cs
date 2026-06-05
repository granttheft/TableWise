using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed class GetPlatformStatsQueryHandler : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    private readonly IApplicationDbContext _db;

    public GetPlatformStatsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _db.Tenants
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted)
            .Select(t => new { t.PlanStatus, t.CreatedAt })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // MRR: aktif subscription'ların plan fiyatları toplamı
        var mrr = await _db.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => !s.IsDeleted && s.Status == PlanStatus.Active)
            .Include(s => s.Plan)
            .SumAsync(s => s.Plan != null ? s.Plan.MonthlyPriceTry : 0m, cancellationToken);

        return new PlatformStatsDto
        {
            TotalTenants = tenants.Count,
            ActiveTenants = tenants.Count(t => t.PlanStatus == PlanStatus.Active),
            TrialTenants = tenants.Count(t => t.PlanStatus == PlanStatus.Trial),
            SuspendedTenants = tenants.Count(t => t.PlanStatus == PlanStatus.Suspended),
            NewTenantsThisMonth = tenants.Count(t => t.CreatedAt >= monthStart),
            Mrr = mrr
        };
    }
}
