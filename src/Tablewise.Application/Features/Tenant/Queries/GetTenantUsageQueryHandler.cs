using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Tenant kullanım istatistikleri sorgusu handler'ı.
/// </summary>
public sealed class GetTenantUsageQueryHandler : IRequestHandler<GetTenantUsageQuery, TenantUsageDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IPlanLimitService _planLimitService;

    public GetTenantUsageQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        IPlanLimitService planLimitService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _planLimitService = planLimitService;
    }

    public async Task<TenantUsageDto> Handle(GetTenantUsageQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Tenant bilgilerini al (Plan ile birlikte)
        var tenant = await _dbContext.Tenants
            .Include(t => t.Plan)
            .Where(t => t.Id == tenantId)
            .Select(t => new
            {
                t.ReservationCountThisMonth,
                t.Plan!.Tier
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new Domain.Exceptions.NotFoundException("Tenant", tenantId);
        }

        // Mevcut kullanımları say
        var venueCount = await _dbContext.Venues
            .CountAsync(v => v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var userCount = await _dbContext.Users
            .CountAsync(u => u.TenantId == tenantId && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var tableCount = await _dbContext.Tables
            .CountAsync(t => t.TenantId == tenantId && !t.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        // Plan limitlerini al
        var planTier = tenant.Tier;

        return new TenantUsageDto
        {
            ReservationsThisMonth = tenant.ReservationCountThisMonth,
            MonthlyReservationLimit = _planLimitService.GetMonthlyReservationLimit(planTier),
            VenueCount = venueCount,
            VenueLimit = _planLimitService.GetVenueLimit(planTier),
            UserCount = userCount,
            UserLimit = _planLimitService.GetUserLimit(planTier),
            TableCount = tableCount,
            TableLimit = _planLimitService.GetTableLimit(planTier)
        };
    }
}
