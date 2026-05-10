using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Queries;

/// <summary>
/// ID'ye göre kural getirme query handler'ı.
/// </summary>
public sealed class GetRuleByIdQueryHandler : IRequestHandler<GetRuleByIdQuery, RuleDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetRuleByIdQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<RuleDto> Handle(GetRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var rule = await _dbContext.Rules
            .Where(r => r.Id == request.RuleId && r.TenantId == tenantId && !r.IsDeleted)
            .AsNoTracking()
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
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rule == null)
        {
            throw new NotFoundException("Rule", request.RuleId);
        }

        return rule;
    }
}
