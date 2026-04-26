using MediatR;
using Tablewise.Application.DTOs.VenueCustomField;

namespace Tablewise.Application.Features.VenueCustomField.Queries;

/// <summary>
/// Venue custom field listesi sorgusu.
/// </summary>
public sealed record GetVenueCustomFieldsQuery : IRequest<List<VenueCustomFieldDto>>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }
}
