using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// <see cref="GetTenantWeeklyChartQuery"/> işleyicisi.
/// </summary>
public sealed class GetTenantWeeklyChartQueryHandler
    : IRequestHandler<GetTenantWeeklyChartQuery, List<WeeklyChartPointDto>>
{
    private const int HeuristicCoversPerTablePerDay = 4;

    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Handler oluşturur.
    /// </summary>
    public GetTenantWeeklyChartQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<List<WeeklyChartPointDto>> Handle(
        GetTenantWeeklyChartQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var todayUtc = DateTime.UtcNow.Date;
        var startUtc = todayUtc.AddDays(-6);

        var tableCount = await _dbContext.Tables
            .CountAsync(t => t.TenantId == tenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var list = new List<WeeklyChartPointDto>(7);

        for (var d = 0; d < 7; d++)
        {
            var dayStart = startUtc.AddDays(d);
            var dayEnd = dayStart.AddDays(1);

            var count = await _dbContext.Reservations
                .Where(r =>
                    r.TenantId == tenantId &&
                    !r.IsDeleted &&
                    r.Status != ReservationStatus.Cancelled &&
                    r.ReservedFor >= dayStart &&
                    r.ReservedFor < dayEnd)
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);

            var occupancy = tableCount <= 0
                ? 0
                : (int)Math.Round(count * 100.0 / Math.Max(1, tableCount * HeuristicCoversPerTablePerDay));

            list.Add(new WeeklyChartPointDto
            {
                Date = dayStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ReservationCount = count,
                OccupancyRate = Math.Clamp(occupancy, 0, 100),
            });
        }

        return list;
    }
}
