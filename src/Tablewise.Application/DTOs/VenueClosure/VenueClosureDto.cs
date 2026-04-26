namespace Tablewise.Application.DTOs.VenueClosure;

/// <summary>
/// Venue kapalılık DTO'su.
/// </summary>
public sealed record VenueClosureDto
{
    /// <summary>
    /// Kapalılık ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kapalılık tarihi.
    /// </summary>
    public required DateTime Date { get; init; }

    /// <summary>
    /// Tüm gün kapalı mı?
    /// </summary>
    public required bool IsFullDay { get; init; }

    /// <summary>
    /// Kısmi kapalılık için açılış saati (HH:mm formatında).
    /// </summary>
    public string? OpenTime { get; init; }

    /// <summary>
    /// Kısmi kapalılık için kapanış saati (HH:mm formatında).
    /// </summary>
    public string? CloseTime { get; init; }

    /// <summary>
    /// Kapalılık nedeni.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
