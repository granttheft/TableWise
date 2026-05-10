namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Dashboard haftalık grafik için günlük nokta.
/// </summary>
public sealed record WeeklyChartPointDto
{
    /// <summary>
    /// Gün (yyyy-MM-dd, UTC).
    /// </summary>
    public required string Date { get; init; }

    /// <summary>
    /// O günkü rezervasyon sayısı.
    /// </summary>
    public int ReservationCount { get; init; }

    /// <summary>
    /// Basit doluluk göstergesi (0–100); masa sayısına göre normalize edilir.
    /// </summary>
    public int OccupancyRate { get; init; }
}
