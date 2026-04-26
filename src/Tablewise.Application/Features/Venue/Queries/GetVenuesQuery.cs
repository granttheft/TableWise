using MediatR;
using Tablewise.Application.DTOs.Venue;

namespace Tablewise.Application.Features.Venue.Queries;

/// <summary>
/// Venue listesi sorgusu.
/// </summary>
public sealed record GetVenuesQuery : IRequest<List<VenueDto>>
{
    /// <summary>
    /// Sadece aktif venue'ler mi?
    /// </summary>
    public bool ActiveOnly { get; init; } = true;
}
