using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// VIP öncelik kuralı koşulları.
/// </summary>
public sealed class VipPriorityConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Minimum müşteri tier seviyesi. "Gold" veya "VIP".
    /// </summary>
    public string MinTier { get; init; } = "Gold";
}

/// <summary>
/// VIP öncelik kuralı aksiyonları.
/// </summary>
public sealed class VipPriorityActions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// En iyi masayı öner.
    /// </summary>
    public bool SuggestBestTable { get; init; }
}
