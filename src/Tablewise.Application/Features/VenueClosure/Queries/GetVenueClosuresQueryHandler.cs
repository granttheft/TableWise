using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.VenueClosure;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.VenueClosure.Queries;

/// <summary>
/// Venue kapalılık listesi sorgusu handler'ı.
/// </summary>
public sealed class GetVenueClosuresQueryHandler : IRequestHandler<GetVenueClosuresQuery, List<VenueClosureDto>>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetVenueClosuresQueryHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<List<VenueClosureDto>> Handle(GetVenueClosuresQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Venue kontrolü
        var venueExists = await _dbContext.Venues
            .AnyAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!venueExists)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Tarih aralığı (varsayılan: bugünden 1 yıl)
        var startDate = request.StartDate ?? DateTime.UtcNow.Date;
        var endDate = request.EndDate ?? DateTime.UtcNow.Date.AddYears(1);

        var closures = await _dbContext.VenueClosures
            .Where(vc => 
                vc.VenueId == request.VenueId && 
                vc.TenantId == tenantId &&
                !vc.IsDeleted &&
                vc.Date >= startDate &&
                vc.Date <= endDate)
            .OrderBy(vc => vc.Date)
            .Select(vc => new VenueClosureDto
            {
                Id = vc.Id,
                VenueId = vc.VenueId,
                Date = vc.Date,
                IsFullDay = vc.IsFullDay,
                OpenTime = vc.OpenTime.HasValue ? vc.OpenTime.Value.ToString(@"hh\:mm") : null,
                CloseTime = vc.CloseTime.HasValue ? vc.CloseTime.Value.ToString(@"hh\:mm") : null,
                Reason = vc.Reason,
                CreatedAt = vc.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return closures;
    }
}
