using MediatR;

namespace Tablewise.Application.Features.VenueCustomField.Commands;

/// <summary>
/// Custom field silme komutu.
/// </summary>
public sealed record DeleteVenueCustomFieldCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Custom field ID.
    /// </summary>
    public required Guid CustomFieldId { get; init; }
}
