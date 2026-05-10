using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Queries;

/// <summary>
/// Kural istatistikleri getirme query handler'ı.
/// </summary>
public sealed class GetRuleStatsQueryHandler : IRequestHandler<GetRuleStatsQuery, List<RuleStatDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetRuleStatsQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<List<RuleStatDto>> Handle(GetRuleStatsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var query = _dbContext.Rules
            .Where(r => r.TenantId == tenantId && !r.IsDeleted)
            .AsNoTracking();

        // VenueId filtresi
        if (request.VenueId.HasValue)
        {
            query = query.Where(r => r.VenueId == request.VenueId.Value);
        }

        var stats = await query
            .OrderByDescending(r => r.TimesTriggered)
            .ThenBy(r => r.Name)
            .Select(r => new RuleStatDto
            {
                RuleId = r.Id,
                RuleName = r.Name,
                TimesTriggered = r.TimesTriggered,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return stats;
    }
}
