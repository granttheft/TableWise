using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.TableCombination;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.TableCombination.Queries;

/// <summary>
/// Venue kombinasyonları listesi sorgusu handler'ı.
/// </summary>
public sealed class GetTableCombinationsQueryHandler : IRequestHandler<GetTableCombinationsQuery, List<TableCombinationDto>>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetTableCombinationsQueryHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<List<TableCombinationDto>> Handle(GetTableCombinationsQuery request, CancellationToken cancellationToken)
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

        var combinations = await _dbContext.TableCombinations
            .Where(tc => 
                tc.VenueId == request.VenueId && 
                tc.TenantId == tenantId && 
                !tc.IsDeleted)
            .OrderBy(tc => tc.Name)
            .Select(tc => new
            {
                tc.Id,
                tc.VenueId,
                tc.Name,
                tc.TableIds,
                tc.CombinedCapacity,
                tc.IsActive,
                tc.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = combinations.Select(tc =>
        {
            // JSON deserialize
            var tableIds = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(tc.TableIds) ?? new List<Guid>();

            return new TableCombinationDto
            {
                Id = tc.Id,
                VenueId = tc.VenueId,
                Name = tc.Name,
                TableIds = tableIds,
                CombinedCapacity = tc.CombinedCapacity,
                IsActive = tc.IsActive,
                CreatedAt = tc.CreatedAt
            };
        }).ToList();

        return result;
    }
}
