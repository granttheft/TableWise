using MediatR;
using Tablewise.Application.DTOs.Venue;

namespace Tablewise.Application.Features.Venue.Queries;

/// <summary>
/// ID'ye göre venue detay sorgusu.
/// </summary>
public sealed record GetVenueByIdQuery : IRequest<VenueDto>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }
}
