using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Venue;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.Venue.Queries;

/// <summary>
/// ID'ye göre venue detay sorgusu handler'ı.
/// </summary>
public sealed class GetVenueByIdQueryHandler : IRequestHandler<GetVenueByIdQuery, VenueDto>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetVenueByIdQueryHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<VenueDto> Handle(GetVenueByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var venue = await _dbContext.Venues
            .Where(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        return venue;
    }
}
