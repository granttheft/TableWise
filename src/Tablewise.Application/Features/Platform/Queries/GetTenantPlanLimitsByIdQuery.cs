using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Exceptions;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed record GetTenantPlanLimitsByIdQuery(Guid TenantId) : IRequest<PlanLimitsDto>;

public sealed class GetTenantPlanLimitsByIdQueryHandler
    : IRequestHandler<GetTenantPlanLimitsByIdQuery, PlanLimitsDto>
{
    private readonly IApplicationDbContext _db;

    public GetTenantPlanLimitsByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlanLimitsDto> Handle(
        GetTenantPlanLimitsByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .Include(t => t.Plan)
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var tenantId = request.TenantId;

        var venueCount = await _db.Venues
            .IgnoreQueryFilters()
            .Where(v => v.TenantId == tenantId && !v.IsDeleted)
            .CountAsync(cancellationToken);

        var tableCount = await _db.Tables
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == tenantId && !t.IsDeleted)
            .CountAsync(cancellationToken);

        var ruleCount = await _db.Rules
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && !r.IsDeleted)
            .CountAsync(cancellationToken);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var reservationCount = await _db.Reservations
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && r.CreatedAt >= monthStart && !r.IsDeleted)
            .CountAsync(cancellationToken);

        var customLimitsJson = tenant.CustomLimitsJson;
        var limitsJson = tenant.Plan?.LimitsJson;

        return new PlanLimitsDto
        {
            MaxVenues               = ReadLimit(customLimitsJson, limitsJson, "maxVenues"),
            CurrentVenueCount       = venueCount,
            MaxTables               = ReadLimit(customLimitsJson, limitsJson, "maxTables"),
            CurrentTableCount       = tableCount,
            MaxRules                = ReadLimit(customLimitsJson, limitsJson, "maxRules"),
            CurrentRuleCount        = ruleCount,
            MaxReservationsPerMonth = ReadLimit(customLimitsJson, limitsJson, "maxReservationsPerMonth"),
            CurrentReservationCount = reservationCount,
            HasCustomLimits         = !string.IsNullOrWhiteSpace(customLimitsJson) && customLimitsJson != "{}",
        };
    }

    private static int? ReadLimit(string? customJson, string? planJson, string key)
    {
        var custom = TryRead(customJson, key);
        if (custom.HasValue) return Normalize(custom.Value);
        var fromPlan = TryRead(planJson, key);
        return fromPlan.HasValue ? Normalize(fromPlan.Value) : null;
    }

    private static int? Normalize(int v) => v < 0 ? null : v;

    private static int? TryRead(string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
            if (!doc.RootElement.TryGetProperty(key, out var prop)) return null;
            return prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var i) ? i : null;
        }
        catch (JsonException) { return null; }
    }
}
