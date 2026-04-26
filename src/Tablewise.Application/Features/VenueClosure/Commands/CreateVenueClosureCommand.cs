using MediatR;

namespace Tablewise.Application.Features.VenueClosure.Commands;

/// <summary>
/// Venue kapalılık oluşturma komutu.
/// StartDate ile EndDate arası her gün için kapalılık kaydı oluşturur.
/// </summary>
public sealed record CreateVenueClosureCommand : IRequest<List<Guid>>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kapalılık başlangıç tarihi.
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// Kapalılık bitiş tarihi.
    /// </summary>
    public required DateTime EndDate { get; init; }

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
