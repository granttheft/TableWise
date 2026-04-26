using MediatR;

namespace Tablewise.Application.Features.VenueClosure.Commands;

/// <summary>
/// Venue kapalılık silme komutu.
/// </summary>
public sealed record DeleteVenueClosureCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kapalılık ID.
    /// </summary>
    public required Guid ClosureId { get; init; }
}
