using MediatR;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Features.Booking.Queries;

/// <summary>
/// Booking UI için mekan yapılandırmasını getiren sorgu.
/// </summary>
public sealed record GetVenueConfigQuery : IRequest<VenueConfigDto>
{
    /// <summary>
    /// Tenant slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;
}
