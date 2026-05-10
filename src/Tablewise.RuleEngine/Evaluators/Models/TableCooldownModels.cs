using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// Masa bekleme süresi kuralı koşulları.
/// </summary>
public sealed class TableCooldownConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Bekleme süresi (dakika).
    /// Bir rezervasyon bittikten sonra aynı masaya yeni rezervasyon için beklenecek süre.
    /// </summary>
    public int CooldownMinutes { get; init; }
}

/// <summary>
/// Masa bekleme süresi kuralı aksiyonları.
/// </summary>
public sealed class TableCooldownActions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Engelleme durumunda gösterilecek mesaj.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
