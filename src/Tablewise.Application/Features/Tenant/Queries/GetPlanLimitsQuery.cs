using System.Text.Json;
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
            .Where(t => t.TenantId == tenantId && !t.IsDeleted)
            .CountAsync(cancellationToken);

        var ruleCount = await _context.Rules
            .Where(r => r.TenantId == tenantId && !r.IsDeleted)
            .CountAsync(cancellationToken);

        // Bu ay rezervasyon sayısı
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var reservationCount = await _context.Reservations
            .Where(r => r.TenantId == tenantId && r.CreatedAt >= monthStart && !r.IsDeleted)
            .CountAsync(cancellationToken);

        var customLimitsJson = tenant.CustomLimitsJson;
        var limitsJson = tenant.Plan?.LimitsJson;

        return new PlanLimitsDto
        {
            MaxVenues = ReadPlanLimit(customLimitsJson, limitsJson, "maxVenues"),
            CurrentVenueCount = venueCount,
            MaxTables = ReadPlanLimit(customLimitsJson, limitsJson, "maxTables"),
            CurrentTableCount = tableCount,
            MaxRules = ReadPlanLimit(customLimitsJson, limitsJson, "maxRules"),
            CurrentRuleCount = ruleCount,
            MaxReservationsPerMonth = ReadPlanLimit(customLimitsJson, limitsJson, "maxReservationsPerMonth"),
            CurrentReservationCount = reservationCount,
            HasCustomLimits = !string.IsNullOrWhiteSpace(customLimitsJson) && customLimitsJson != "{}"
        };
    }

    /// <summary>
    /// Limit çözümleme: önce customLimitsJson (tenant özel), sonra planLimitsJson (plan geneli).
    /// Negatif değer sınırsız kabul edilir (null döner).
    /// </summary>
    private static int? ReadPlanLimit(string? customLimitsJson, string? planLimitsJson, string key)
    {
        var custom = TryReadIntProperty(customLimitsJson, key);
        if (custom.HasValue)
            return NormalizeLimit(custom.Value);

        var fromPlan = TryReadIntProperty(planLimitsJson, key);
        return fromPlan.HasValue ? NormalizeLimit(fromPlan.Value) : null;
    }

    private static int? NormalizeLimit(int value) => value < 0 ? null : value;

    private static int? TryReadIntProperty(string? json, string key)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!doc.RootElement.TryGetProperty(key, out var prop))
            {
                return null;
            }

            return prop.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Number => prop.TryGetInt32(out var i) ? i : null,
                _ => null
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
