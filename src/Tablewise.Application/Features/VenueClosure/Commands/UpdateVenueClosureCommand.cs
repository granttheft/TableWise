using MediatR;

namespace Tablewise.Application.Features.VenueClosure.Commands;

/// <summary>
/// Venue kapalılık güncelleme komutu.
/// </summary>
public sealed record UpdateVenueClosureCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kapalılık ID.
    /// </summary>
    public required Guid ClosureId { get; init; }

    /// <summary>
    /// Kapalılık tarihi.
    /// </summary>
    public required DateTime Date { get; init; }

    /// <summary>
    /// Tüm gün kapalı mı?
    /// </summary>
    public required bool IsFullDay { get; init; }

    /// <summary>
    /// Kısmi kapalılık için açılış saati.
    /// </summary>
    public TimeSpan? OpenTime { get; init; }

    /// <summary>
    /// Kısmi kapalılık için kapanış saati.
    /// </summary>
    public TimeSpan? CloseTime { get; init; }

    /// <summary>
    /// Kapalılık nedeni.
    /// </summary>
    public string? Reason { get; init; }
}
