using MediatR;

namespace Tablewise.Application.Features.Table.Commands;

/// <summary>
/// Masa aktiflik durumu toggle komutu.
/// IsActive'i tersine çevirir (true → false, false → true).
/// </summary>
public sealed record ToggleTableActiveCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Masa ID.
    /// </summary>
    public required Guid TableId { get; init; }
}
