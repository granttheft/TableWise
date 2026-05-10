namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Admin panel dashboard üst istatistik kartları için özet DTO.
/// </summary>
public sealed record TenantDashboardStatsDto
{
    /// <summary>
    /// Bugünkü rezervasyon adedi (UTC günü).
    /// </summary>
    public int TodayReservations { get; init; }

    /// <summary>
    /// Düne göre bugünkü rezervasyon farkı (bugün − dün).
    /// </summary>
    public int TodayReservationsChange { get; init; }

    /// <summary>
    /// Son 7 gündeki rezervasyonlara göre tahmini doluluk yüzdesi (0–100).
    /// </summary>
    public int WeekOccupancyRate { get; init; }

    /// <summary>
    /// Bir önceki 7 güne göre rezervasyon adedi farkı (gösterim için).
    /// </summary>
    public int WeekOccupancyRateChange { get; init; }

    /// <summary>
    /// Takvim ayı içindeki rezervasyon sayısı (UTC).
    /// </summary>
    public int MonthReservations { get; init; }

    /// <summary>
    /// Aylık plan limiti; sınırsız planda null.
    /// </summary>
    public int? MonthReservationsLimit { get; init; }

    /// <summary>
    /// Aktif kural sayısı.
    /// </summary>
    public int ActiveRulesCount { get; init; }
}
