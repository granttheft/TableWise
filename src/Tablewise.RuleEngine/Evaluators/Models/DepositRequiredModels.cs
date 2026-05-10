using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// Kapora gereksinimi kuralı koşulları.
/// </summary>
public sealed class DepositRequiredConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Kapsam tanımları (tümü opsiyonel).
    /// </summary>
    public DepositScopes? Scopes { get; init; }
}

/// <summary>
/// Kapora kapsamları.
/// </summary>
public sealed class DepositScopes
{
    /// <summary>
    /// Hangi günlerde geçerli (İngilizce gün adları: "Friday", "Saturday" vb.).
    /// </summary>
    public string[]? Days { get; init; }

    /// <summary>
    /// Hangi saatlerde geçerli ("19:00", "20:00" formatında).
    /// </summary>
    public string[]? Times { get; init; }

    /// <summary>
    /// Hangi masalar için geçerli (Guid string listesi).
    /// </summary>
    public string[]? TableIds { get; init; }

    /// <summary>
    /// Minimum kişi sayısı.
    /// </summary>
    public int? MinPartySize { get; init; }
}

/// <summary>
/// Kapora gereksinimi kuralı aksiyonları.
/// </summary>
public sealed class DepositRequiredActions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? Amount { get; init; }

    /// <summary>
    /// Kişi başı mı hesaplansın?
    /// </summary>
    public bool PerPerson { get; init; }

    /// <summary>
    /// Mekanın varsayılan kapora tutarını kullan.
    /// </summary>
    public bool UseVenueDefault { get; init; }
}
