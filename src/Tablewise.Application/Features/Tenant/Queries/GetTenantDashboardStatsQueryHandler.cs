using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// <see cref="GetTenantDashboardStatsQuery"/> işleyicisi.
/// </summary>
public sealed class GetTenantDashboardStatsQueryHandler
    : IRequestHandler<GetTenantDashboardStatsQuery, TenantDashboardStatsDto>
{
    private const int HeuristicCoversPerTablePerDay = 4;

    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IPlanLimitService _planLimitService;

    /// <summary>
    /// Handler oluşturur.
    /// </summary>
    public GetTenantDashboardStatsQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        IPlanLimitService planLimitService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _planLimitService = planLimitService;
    }

    /// <inheritdoc />
    public async Task<TenantDashboardStatsDto> Handle(
        GetTenantDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .Include(t => t.Plan)
            .Where(t => t.Id == tenantId)
            .Select(t => new { t.Plan!.Tier })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new Domain.Exceptions.NotFoundException("Tenant", tenantId);
        }

        var planTier = tenant.Tier;

        // Rezervasyon saatleri UTC saklanıyor; Istanbul lokal zamanına göre gün sınırları belirlenmeli
        var tz = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Turkey Standard Time" : "Europe/Istanbul");
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var todayLocalStart = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var todayUtc = TimeZoneInfo.ConvertTimeToUtc(todayLocalStart, tz);
        var tomorrowUtc = todayUtc.AddDays(1);
        var yesterdayUtc = todayUtc.AddDays(-1);

        var todayOnly = await CountReservationsAsync(
                tenantId,
                todayUtc,
                tomorrowUtc,
                cancellationToken)
            .ConfigureAwait(false);

        var yesterdayOnly = await CountReservationsAsync(
                tenantId,
                yesterdayUtc,
                todayUtc,
                cancellationToken)
            .ConfigureAwait(false);

        var monthLocalStart = new DateTime(nowLocal.Year, nowLocal.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var monthStart = TimeZoneInfo.ConvertTimeToUtc(monthLocalStart, tz);
        var monthEnd = TimeZoneInfo.ConvertTimeToUtc(monthLocalStart.AddMonths(1), tz);

        var monthCount = await CountReservationsAsync(
                tenantId,
                monthStart,
                monthEnd,
                cancellationToken)
            .ConfigureAwait(false);

        var monthlyLimit = _planLimitService.GetMonthlyReservationLimit(planTier);
        int? monthLimitNullable = monthlyLimit < 0 ? null : monthlyLimit;

        var activeRulesCount = await _dbContext.Rules
            .CountAsync(
                r => r.TenantId == tenantId && !r.IsDeleted && r.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        var tableCount = await _dbContext.Tables
            .CountAsync(t => t.TenantId == tenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var rolling7Start = todayUtc.AddDays(-6);
        var rolling7PrevStart = todayUtc.AddDays(-13);
        var rolling7PrevEnd = todayUtc.AddDays(-6);

        var thisWeekReservations = await CountReservationsAsync(
                tenantId,
                rolling7Start,
                tomorrowUtc,
                cancellationToken)
            .ConfigureAwait(false);

        var prevWeekReservations = await CountReservationsAsync(
                tenantId,
                rolling7PrevStart,
                rolling7PrevEnd,
                cancellationToken)
            .ConfigureAwait(false);

        var weekOccupancyRate = ComputeWeekOccupancyRate(thisWeekReservations, tableCount);

        return new TenantDashboardStatsDto
        {
            TodayReservations = todayOnly,
            TodayReservationsChange = todayOnly - yesterdayOnly,
            WeekOccupancyRate = weekOccupancyRate,
            WeekOccupancyRateChange = thisWeekReservations - prevWeekReservations,
            MonthReservations = monthCount,
            MonthReservationsLimit = monthLimitNullable,
            ActiveRulesCount = activeRulesCount,
        };
    }

    private static int ComputeWeekOccupancyRate(int reservationCountLast7Days, int tableCount)
    {
        if (tableCount <= 0 || reservationCountLast7Days <= 0)
        {
            return 0;
        }

        var denominator = Math.Max(1, tableCount * 7 * HeuristicCoversPerTablePerDay);
        var rate = (int)Math.Round(reservationCountLast7Days * 100.0 / denominator);
        return Math.Clamp(rate, 0, 100);
    }

    private Task<int> CountReservationsAsync(
        Guid tenantId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken)
    {
        return _dbContext.Reservations
            .Where(r =>
                r.TenantId == tenantId &&
                !r.IsDeleted &&
                r.Status != ReservationStatus.Cancelled &&
                r.ReservedFor >= fromInclusive &&
                r.ReservedFor < toExclusive)
            .CountAsync(cancellationToken);
    }
}
