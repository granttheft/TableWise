using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Services;

/// <summary>
/// Plan limit kontrol servisi implementation.
/// </summary>
public sealed class PlanLimitService : IPlanLimitService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<PlanLimitService> _logger;

    /// <summary>
    /// PlanLimitService constructor.
    /// </summary>
    public PlanLimitService(
        IApplicationDbContext dbContext,
        ILogger<PlanLimitService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task CheckVenueLimitAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var limits = await GetPlanLimitsAsync(tenantId, cancellationToken).ConfigureAwait(false);
        
        if (limits.MaxVenues < 0) return; // Sınırsız

        var currentCount = await _dbContext.Venues
            .CountAsync(v => v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (currentCount >= limits.MaxVenues)
        {
            throw new PlanLimitExceededException(
                "MaxVenues",
                limits.MaxVenues,
                "https://app.tablewise.com.tr/settings/subscription/upgrade",
                $"Planınızın mekan limiti ({limits.MaxVenues}) dolmuş. Yeni mekan eklemek için planınızı yükseltin.");
        }
    }

    /// <inheritdoc />
    public async Task CheckTableLimitAsync(Guid tenantId, Guid? venueId = null, CancellationToken cancellationToken = default)
    {
        var limits = await GetPlanLimitsAsync(tenantId, cancellationToken).ConfigureAwait(false);
        
        if (limits.MaxTables < 0) return; // Sınırsız

        var currentCount = await _dbContext.Tables
            .CountAsync(t => t.TenantId == tenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (currentCount >= limits.MaxTables)
        {
            throw new PlanLimitExceededException(
                "MaxTables",
                limits.MaxTables,
                "https://app.tablewise.com.tr/settings/subscription/upgrade",
                $"Planınızın masa limiti ({limits.MaxTables}) dolmuş. Yeni masa eklemek için planınızı yükseltin.");
        }
    }

    /// <inheritdoc />
    public async Task CheckRuleLimitAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var limits = await GetPlanLimitsAsync(tenantId, cancellationToken).ConfigureAwait(false);
        
        if (limits.MaxRules < 0) return; // Sınırsız

        var currentCount = await _dbContext.Rules
            .CountAsync(r => r.TenantId == tenantId && !r.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (currentCount >= limits.MaxRules)
        {
            throw new PlanLimitExceededException(
                "MaxRules",
                limits.MaxRules,
                "https://app.tablewise.com.tr/settings/subscription/upgrade",
                $"Planınızın kural limiti ({limits.MaxRules}) dolmuş. Yeni kural eklemek için planınızı yükseltin.");
        }
    }

    /// <inheritdoc />
    public async Task CheckReservationLimitAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var limits = await GetPlanLimitsAsync(tenantId, cancellationToken).ConfigureAwait(false);
        
        if (limits.MaxReservationsPerMonth < 0) return; // Sınırsız

        // Tenant entity'den bu ay rezervasyon sayısını al
        var tenant = await _dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.ReservationCountThisMonth)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tenant >= limits.MaxReservationsPerMonth)
        {
            throw new PlanLimitExceededException(
                "MaxReservationsPerMonth",
                limits.MaxReservationsPerMonth,
                "https://app.tablewise.com.tr/settings/subscription/upgrade",
                $"Bu ay {limits.MaxReservationsPerMonth} rezervasyon limitinize ulaştınız. Daha fazla rezervasyon almak için planınızı yükseltin.");
        }
    }

    /// <inheritdoc />
    public async Task<PlanUsageSummaryDto> GetTenantUsageAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var limits = await GetPlanLimitsAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var venueCount = await _dbContext.Venues
            .CountAsync(v => v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var tableCount = await _dbContext.Tables
            .CountAsync(t => t.TenantId == tenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var ruleCount = await _dbContext.Rules
            .CountAsync(r => r.TenantId == tenantId && !r.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var reservationCount = await _dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.ReservationCountThisMonth)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PlanUsageSummaryDto
        {
            Venues = new UsageInfo
            {
                Current = venueCount,
                Maximum = limits.MaxVenues
            },
            Tables = new UsageInfo
            {
                Current = tableCount,
                Maximum = limits.MaxTables
            },
            Rules = new UsageInfo
            {
                Current = ruleCount,
                Maximum = limits.MaxRules
            },
            ReservationsThisMonth = new UsageInfo
            {
                Current = reservationCount,
                Maximum = limits.MaxReservationsPerMonth
            }
        };
    }

    /// <inheritdoc />
    public int GetMonthlyReservationLimit(Domain.Enums.PlanTier tier)
    {
        return tier switch
        {
            Domain.Enums.PlanTier.Starter => 100,
            Domain.Enums.PlanTier.Pro => 500,
            Domain.Enums.PlanTier.Business => 2000,
            Domain.Enums.PlanTier.Enterprise => -1, // Sınırsız
            _ => 100
        };
    }

    /// <inheritdoc />
    public int GetVenueLimit(Domain.Enums.PlanTier tier)
    {
        return tier switch
        {
            Domain.Enums.PlanTier.Starter => 1,
            Domain.Enums.PlanTier.Pro => 1,
            Domain.Enums.PlanTier.Business => 3,
            Domain.Enums.PlanTier.Enterprise => -1, // Sınırsız
            _ => 1
        };
    }

    /// <inheritdoc />
    public int GetUserLimit(Domain.Enums.PlanTier tier)
    {
        return tier switch
        {
            Domain.Enums.PlanTier.Starter => 2,
            Domain.Enums.PlanTier.Pro => 5,
            Domain.Enums.PlanTier.Business => 20,
            Domain.Enums.PlanTier.Enterprise => -1, // Sınırsız
            _ => 2
        };
    }

    /// <inheritdoc />
    public int GetTableLimit(Domain.Enums.PlanTier tier)
    {
        return tier switch
        {
            Domain.Enums.PlanTier.Starter => 3,
            Domain.Enums.PlanTier.Pro => -1, // Sınırsız
            Domain.Enums.PlanTier.Business => -1, // Sınırsız
            Domain.Enums.PlanTier.Enterprise => -1, // Sınırsız
            _ => 3
        };
    }

    /// <summary>
    /// Tenant'ın plan limitlerini alır.
    /// </summary>
    private async Task<PlanLimits> GetPlanLimitsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var planJson = await _dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Join(_dbContext.Plans, t => t.PlanId, p => p.Id, (t, p) => new { p.FeaturesJson, p.LimitsJson })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (planJson is null)
        {
            _logger.LogWarning("Tenant {TenantId} için plan bulunamadı, varsayılan limitler kullanılıyor", tenantId);
            return PlanLimits.Default;
        }

        // Seed maxTables/maxRules değerleri FeaturesJson içinde; LimitsJson teknik kota (apiRateLimit vb.).
        return new PlanLimits
        {
            MaxVenues = ResolvePlanLimit(planJson.FeaturesJson, planJson.LimitsJson, "maxVenues", PlanLimits.Default.MaxVenues),
            MaxTables = ResolvePlanLimit(planJson.FeaturesJson, planJson.LimitsJson, "maxTables", PlanLimits.Default.MaxTables),
            MaxRules = ResolvePlanLimit(planJson.FeaturesJson, planJson.LimitsJson, "maxRules", PlanLimits.Default.MaxRules),
            MaxReservationsPerMonth = ResolvePlanLimit(
                planJson.FeaturesJson,
                planJson.LimitsJson,
                "maxReservationsPerMonth",
                PlanLimits.Default.MaxReservationsPerMonth)
        };
    }

    /// <summary>
    /// Plan limit anahtarını okur: önce FeaturesJson, sonra LimitsJson. Negatif değer sınırsız (-1).
    /// </summary>
    private static int ResolvePlanLimit(string? featuresJson, string? limitsJson, string key, int defaultWhenMissing)
    {
        var fromFeatures = TryReadIntProperty(featuresJson, key);
        if (fromFeatures.HasValue)
        {
            return fromFeatures.Value < 0 ? -1 : fromFeatures.Value;
        }

        var fromLimits = TryReadIntProperty(limitsJson, key);
        if (fromLimits.HasValue)
        {
            return fromLimits.Value < 0 ? -1 : fromLimits.Value;
        }

        return defaultWhenMissing;
    }

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

    /// <summary>
    /// Plan limitleri JSON model.
    /// </summary>
    private sealed record PlanLimits
    {
        public int MaxVenues { get; init; }
        public int MaxTables { get; init; }
        public int MaxRules { get; init; }
        public int MaxReservationsPerMonth { get; init; }

        public static PlanLimits Default => new()
        {
            MaxVenues = 1,
            MaxTables = 3,
            MaxRules = 5,
            MaxReservationsPerMonth = 100
        };
    }
}
