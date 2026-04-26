namespace Tablewise.Application.DTOs.Venue;

/// <summary>
/// Venue çalışma saatleri güncelleme DTO'su.
/// </summary>
public sealed record UpdateWorkingHoursDto
{
    /// <summary>
    /// Çalışma saatleri (JSON formatında).
    /// Format: { "Monday": { "isOpen": true, "openTime": "10:00", "closeTime": "22:00" }, ... }
    /// </summary>
    public required string WorkingHours { get; init; }
}
