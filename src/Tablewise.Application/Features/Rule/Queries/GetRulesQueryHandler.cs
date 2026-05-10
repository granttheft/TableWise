using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Queries;

/// <summary>
/// Kural listesi getirme query handler'ı.
/// </summary>
public sealed class GetRulesQueryHandler : IRequestHandler<GetRulesQuery, List<RuleDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetRulesQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<List<RuleDto>> Handle(GetRulesQuery request, CancellationToken cancellationToken)
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

        // IsActive filtresi
        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        // TriggerType filtresi
        if (request.TriggerType.HasValue)
        {
            query = query.Where(r => r.TriggerType == request.TriggerType.Value);
        }

        // Venue Name join
        var rules = await query
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.Name)
            .Select(r => new RuleDto
            {
                Id = r.Id,
                VenueId = r.VenueId,
                VenueName = r.VenueId.HasValue
                    ? _dbContext.Venues
                        .Where(v => v.Id == r.VenueId.Value)
                        .Select(v => v.Name)
                        .FirstOrDefault()
                    : null,
                Name = r.Name,
                Description = r.Description,
                RuleType = r.RuleType,
                ConditionsJson = r.ConditionsJson,
                ActionsJson = r.ActionsJson,
                Priority = r.Priority,
                TriggerType = r.TriggerType,
                IsActive = r.IsActive,
                ApplicableTimeSlots = r.ApplicableTimeSlots,
                TimesTriggered = r.TimesTriggered,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rules;
    }
}
