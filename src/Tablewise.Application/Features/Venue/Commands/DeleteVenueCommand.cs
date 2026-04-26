using MediatR;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue silme komutu (soft delete).
/// Sadece Owner rolü kullanabilir.
/// </summary>
public sealed record DeleteVenueCommand : IRequest<Unit>
{
    /// <summary>
    /// Silinecek venue ID'si.
    /// </summary>
    public required Guid VenueId { get; init; }
}
