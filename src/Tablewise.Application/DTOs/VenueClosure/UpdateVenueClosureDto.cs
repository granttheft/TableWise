namespace Tablewise.Application.DTOs.VenueClosure;

/// <summary>
/// Venue kapalılık güncelleme DTO'su.
/// </summary>
public sealed record UpdateVenueClosureDto
{
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
}
