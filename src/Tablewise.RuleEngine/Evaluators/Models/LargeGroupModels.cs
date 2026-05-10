using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// Büyük grup kuralı koşulları.
/// </summary>
public sealed class LargeGroupConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Minimum kişi sayısı (bu sayı ve üzeri büyük grup sayılır).
    /// </summary>
    public int MinPartySize { get; init; }
}

/// <summary>
/// Büyük grup kuralı aksiyonları.
/// </summary>
public sealed class LargeGroupActions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Masa birleşimi gerekli mi?
    /// </summary>
    public bool RequireCombination { get; init; }

    /// <summary>
    /// Engelleme durumunda gösterilecek mesaj.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
