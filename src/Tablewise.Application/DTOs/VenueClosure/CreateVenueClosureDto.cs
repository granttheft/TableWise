namespace Tablewise.Application.DTOs.VenueClosure;

/// <summary>
/// Venue kapalılık oluşturma DTO'su.
/// </summary>
public sealed record CreateVenueClosureDto
{
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
    public bool IsFullDay { get; init; } = true;

    /// <summary>
    /// Kısmi kapalılık için açılış saati (HH:mm formatında, örn: "10:00").
    /// </summary>
    public string? OpenTime { get; init; }

    /// <summary>
    /// Kısmi kapalılık için kapanış saati (HH:mm formatında, örn: "14:00").
    /// </summary>
    public string? CloseTime { get; init; }

    /// <summary>
    /// Kapalılık nedeni (opsiyonel).
    /// </summary>
    public string? Reason { get; init; }
}
