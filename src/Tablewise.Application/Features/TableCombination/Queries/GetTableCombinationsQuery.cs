using MediatR;
using Tablewise.Application.DTOs.TableCombination;

namespace Tablewise.Application.Features.TableCombination.Queries;

/// <summary>
/// Venue kombinasyonları listesi sorgusu.
/// </summary>
public sealed record GetTableCombinationsQuery : IRequest<List<TableCombinationDto>>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }
}
