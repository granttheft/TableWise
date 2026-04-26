namespace Tablewise.Application.DTOs.Venue;

/// <summary>
/// Haftalık çalışma saatleri DTO'su.
/// </summary>
public sealed record WorkingHoursDto
{
    /// <summary>
    /// Günlük çalışma saatleri (key: DayOfWeek, value: DayWorkingHours).
    /// </summary>
    public required Dictionary<string, DayWorkingHours> Days { get; init; }
}

/// <summary>
/// Günlük çalışma saatleri.
/// </summary>
public sealed record DayWorkingHours
{
    /// <summary>
    /// Bu gün açık mı?
    /// </summary>
    public required bool IsOpen { get; init; }

    /// <summary>
    /// Açılış saati (HH:mm formatında, örn: "10:00").
    /// </summary>
    public string? OpenTime { get; init; }

    /// <summary>
    /// Kapanış saati (HH:mm formatında, örn: "22:00").
    /// </summary>
    public string? CloseTime { get; init; }
}
