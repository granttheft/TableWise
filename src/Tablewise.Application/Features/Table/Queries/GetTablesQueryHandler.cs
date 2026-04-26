using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Table;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Table.Queries;

/// <summary>
/// Venue masaları listesi sorgusu handler'ı.
/// </summary>
public sealed class GetTablesQueryHandler : IRequestHandler<GetTablesQuery, List<TableDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetTablesQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<List<TableDto>> Handle(GetTablesQuery request, CancellationToken cancellationToken)
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

        var query = _dbContext.Tables
            .Where(t => 
                t.VenueId == request.VenueId && 
                t.TenantId == tenantId && 
                !t.IsDeleted);

        if (request.ActiveOnly)
        {
            query = query.Where(t => t.IsActive);
        }

        var tables = await query
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .Select(t => new TableDto
            {
                Id = t.Id,
                VenueId = t.VenueId,
                Name = t.Name,
                Capacity = t.Capacity,
                Location = t.Location,
                Description = t.Description,
                SortOrder = t.SortOrder,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return tables;
    }
}
