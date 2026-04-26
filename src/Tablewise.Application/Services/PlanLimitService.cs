using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Services;

/// <summary>
/// Plan limit kontrol servisi implementation.
/// </summary>
public sealed class PlanLimitService : IPlanLimitService
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ILogger<PlanLimitService> _logger;

    /// <summary>
    /// PlanLimitService constructor.
    /// </summary>
    public PlanLimitService(
        TablewiseDbContext dbContext,
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
    public async Task<TenantUsageDto> GetTenantUsageAsync(Guid tenantId, CancellationToken cancellationToken = default)
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

        return new TenantUsageDto
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

    /// <summary>
    /// Tenant'ın plan limitlerini alır.
    /// </summary>
    private async Task<PlanLimits> GetPlanLimitsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var limitsJson = await _dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Join(_dbContext.Plans, t => t.PlanId, p => p.Id, (t, p) => p.LimitsJson)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(limitsJson))
        {
            _logger.LogWarning("Tenant {TenantId} için plan limitleri bulunamadı, varsayılan limitler kullanılıyor", tenantId);
            return PlanLimits.Default;
        }

        try
        {
            return JsonSerializer.Deserialize<PlanLimits>(limitsJson) ?? PlanLimits.Default;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Plan limitleri JSON parse hatası, varsayılan limitler kullanılıyor");
            return PlanLimits.Default;
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
