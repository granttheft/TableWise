using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// Yoğun saat kuralı koşulları.
/// </summary>
public sealed class PeakHourConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Başlangıç saati ("19:00" formatında).
    /// </summary>
    public string StartTime { get; init; } = "19:00";

    /// <summary>
    /// Bitiş saati ("22:00" formatında).
    /// </summary>
    public string EndTime { get; init; } = "22:00";

    /// <summary>
    /// Minimum doluluk yüzdesi (opsiyonel).
    /// Bu değerin altındaysa kural tetiklenmez.
    /// </summary>
    public int? MinOccupancyPercent { get; init; }

    /// <summary>
    /// Hangi günlerde geçerli (İngilizce gün adları, opsiyonel).
    /// </summary>
    public string[]? Days { get; init; }
}

/// <summary>
/// Yoğun saat kuralı aksiyonları.
/// </summary>
public sealed class PeakHourActions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Rezervasyonu engelle.
    /// </summary>
    public bool Block { get; init; }

    /// <summary>
    /// Uyarı göster (engellemeden).
    /// </summary>
    public bool Warn { get; init; }

    /// <summary>
    /// Gösterilecek mesaj.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
