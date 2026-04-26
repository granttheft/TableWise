using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Venue;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Venue.Queries;

/// <summary>
/// Venue listesi sorgusu handler'ı.
/// </summary>
public sealed class GetVenuesQueryHandler : IRequestHandler<GetVenuesQuery, List<VenueDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetVenuesQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<List<VenueDto>> Handle(GetVenuesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var query = _dbContext.Venues
            .Where(v => v.TenantId == tenantId);

        if (request.ActiveOnly)
        {
            query = query.Where(v => !v.IsDeleted);
        }

        var venues = await query
            .OrderBy(v => v.Name)
            .Select(v => new VenueDto
            {
                Id = v.Id,
                Name = v.Name,
                Address = v.Address,
                PhoneNumber = v.PhoneNumber,
                Description = v.Description,
                TimeZone = v.TimeZone,
                LogoUrl = v.LogoUrl,
                SlotDurationMinutes = v.SlotDurationMinutes,
                DepositEnabled = v.DepositEnabled,
                DepositAmount = v.DepositAmount,
                DepositPerPerson = v.DepositPerPerson,
                DepositRefundPolicy = v.DepositRefundPolicy,
                DepositRefundHours = v.DepositRefundHours,
                DepositPartialPercent = v.DepositPartialPercent,
                WorkingHours = v.WorkingHours,
                TableCount = v.Tables.Count(t => !t.IsDeleted),
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return venues;
    }
}
