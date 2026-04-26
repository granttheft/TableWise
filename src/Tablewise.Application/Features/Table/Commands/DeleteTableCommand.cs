using MediatR;

namespace Tablewise.Application.Features.Table.Commands;

/// <summary>
/// Masa silme komutu (soft delete).
/// Aktif rezervasyonu olan masa silinemez.
/// </summary>
public sealed record DeleteTableCommand : IRequest<Unit>
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
