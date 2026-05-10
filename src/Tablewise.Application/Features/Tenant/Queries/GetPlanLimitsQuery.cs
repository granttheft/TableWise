using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Plan limitleri query'si
/// </summary>
public sealed class GetPlanLimitsQuery : IRequest<PlanLimitsDto>
{
}

/// <summary>
/// GetPlanLimitsQuery handler
/// </summary>
public sealed class GetPlanLimitsQueryHandler : IRequestHandler<GetPlanLimitsQuery, PlanLimitsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetPlanLimitsQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<PlanLimitsDto> Handle(GetPlanLimitsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var tenant = await _context.Tenants
            .Include(t => t.Plan)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant == null)
        {
            throw new InvalidOperationException("Tenant bulunamadı");
        }

        // Mevcut kullanımları hesapla
        var venueCount = await _context.Venues
            .Where(v => v.TenantId == tenantId && !v.IsDeleted)
            .CountAsync(cancellationToken);

        var tableCount = await _context.Tables
            .Where(t => t.Venue.TenantId == tenantId && !t.IsDeleted)
            .CountAsync(cancellationToken);

        var ruleCount = await _context.Rules
            .Where(r => r.Venue.TenantId == tenantId && !r.IsDeleted)
            .CountAsync(cancellationToken);

        // Bu ay rezervasyon sayısı
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var reservationCount = await _context.Reservations
            .Where(r => r.Table.Venue.TenantId == tenantId && 
                       r.CreatedAt >= monthStart && 
                       !r.IsDeleted)
            .CountAsync(cancellationToken);

        var planLimits = tenant.Plan?.FeatureFlagsJson != null
            ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(tenant.Plan.FeatureFlagsJson)
            : new Dictionary<string, object>();

        return new PlanLimitsDto
        {
            MaxVenues = GetLimitFromFlags(planLimits, "maxVenues"),
            CurrentVenueCount = venueCount,
            MaxTables = GetLimitFromFlags(planLimits, "maxTables"),
            CurrentTableCount = tableCount,
            MaxRules = GetLimitFromFlags(planLimits, "maxRules"),
            CurrentRuleCount = ruleCount,
            MaxReservationsPerMonth = GetLimitFromFlags(planLimits, "maxReservationsPerMonth"),
            CurrentReservationCount = reservationCount
        };
    }

    private static int? GetLimitFromFlags(Dictionary<string, object> flags, string key)
    {
        if (flags.TryGetValue(key, out var value))
        {
            if (value is System.Text.Json.JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    return jsonElement.GetInt32();
                }
                if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Null)
                {
                    return null; // Unlimited
                }
            }
        }
        return null; // Unlimited by default
    }
}
