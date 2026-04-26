using MediatR;

namespace Tablewise.Application.Features.TableCombination.Commands;

/// <summary>
/// Masa kombinasyonu silme komutu (soft delete).
/// </summary>
public sealed record DeleteTableCombinationCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kombinasyon ID.
    /// </summary>
    public required Guid CombinationId { get; init; }
}
