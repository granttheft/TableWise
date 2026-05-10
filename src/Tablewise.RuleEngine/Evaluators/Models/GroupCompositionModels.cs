using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// Grup kompozisyonu kuralı koşulları.
/// </summary>
public sealed class GroupCompositionConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Alt kuralların birleşim operatörü: "and" veya "or".
    /// "and" = tüm kurallar ihlal edilmeli, "or" = herhangi biri ihlal edilmeli.
    /// </summary>
    public string Operator { get; init; } = "or";

    /// <summary>
    /// Alt kurallar listesi.
    /// </summary>
    public CompositionRule[] Rules { get; init; } = [];
}

/// <summary>
/// Tek bir kompozisyon alt kuralı.
/// </summary>
public sealed class CompositionRule
{
    /// <summary>
    /// Kural tipi: "composition" veya "ratio".
    /// </summary>
    public string Type { get; init; } = "composition";

    /// <summary>
    /// Engellenen grup kompozisyonları (type="composition" için).
    /// Örn: ["AllMale", "AllFemale"]
    /// </summary>
    public string[]? BlockedCompositions { get; init; }

    /// <summary>
    /// İzin verilen grup kompozisyonları (type="composition" için).
    /// Örn: ["Mixed", "Family"]
    /// </summary>
    public string[]? AllowedCompositions { get; init; }

    /// <summary>
    /// Minimum kadın oranı (type="ratio" için, 0.0-1.0 arası).
    /// </summary>
    public double? MinFemaleRatio { get; init; }

    /// <summary>
    /// Maksimum erkek oranı (type="ratio" için, 0.0-1.0 arası).
    /// </summary>
    public double? MaxMaleRatio { get; init; }

    /// <summary>
    /// Bu kuralın uygulanacağı minimum kişi sayısı.
    /// </summary>
    public int? MinPartySize { get; init; }
}

/// <summary>
/// Grup kompozisyonu kuralı aksiyonları.
/// </summary>
public sealed class GroupCompositionActions : IVersionedJson
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
