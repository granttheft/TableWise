using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.VenueCustomField;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.VenueCustomField.Queries;

/// <summary>
/// Venue custom field listesi sorgusu handler'ı.
/// </summary>
public sealed class GetVenueCustomFieldsQueryHandler : IRequestHandler<GetVenueCustomFieldsQuery, List<VenueCustomFieldDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetVenueCustomFieldsQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<List<VenueCustomFieldDto>> Handle(GetVenueCustomFieldsQuery request, CancellationToken cancellationToken)
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

        var customFields = await _dbContext.VenueCustomFields
            .Where(cf => 
                cf.VenueId == request.VenueId && 
                cf.TenantId == tenantId && 
                !cf.IsDeleted)
            .OrderBy(cf => cf.SortOrder)
            .Select(cf => new VenueCustomFieldDto
            {
                Id = cf.Id,
                VenueId = cf.VenueId,
                Label = cf.Label,
                FieldType = cf.FieldType,
                IsRequired = cf.IsRequired,
                SortOrder = cf.SortOrder,
                Options = cf.Options,
                CreatedAt = cf.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return customFields;
    }
}
