using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// Custom condition kuralı koşulları.
/// İşletmenin kendi mantığını tanımlamasına izin verir.
/// </summary>
public sealed class CustomConditionConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Alt koşulların birleşim operatörü: "and" veya "or".
    /// "and" = tüm koşullar sağlanmalı, "or" = herhangi biri yeterli.
    /// </summary>
    public string Operator { get; init; } = "and";

    /// <summary>
    /// Koşullar listesi.
    /// </summary>
    public CustomCondition[] Conditions { get; init; } = [];
}

/// <summary>
/// Tek bir custom koşul tanımı.
/// </summary>
public sealed class CustomCondition
{
    /// <summary>
    /// Alan adı (whitelist'te olmalı).
    /// Örn: "partySize", "customer.tier", "groupComposition"
    /// </summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>
    /// Karşılaştırma operatörü.
    /// Geçerli değerler: ==, !=, &lt;, &lt;=, &gt;, &gt;=, in, contains
    /// </summary>
    public string Op { get; init; } = "==";

    /// <summary>
    /// Karşılaştırılacak değer.
    /// Tip: int, double, string, string[] (in operatörü için)
    /// </summary>
    public object? Value { get; init; }
}

/// <summary>
/// Custom condition kuralı aksiyonları.
/// </summary>
public sealed class CustomConditionActions : IVersionedJson
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
    /// Uygulayacak indirim yüzdesi (opsiyonel).
    /// </summary>
    public decimal? DiscountPercent { get; init; }

    /// <summary>
    /// Öneri sun.
    /// </summary>
    public bool Suggest { get; init; }

    /// <summary>
    /// Kapora gerekli mi?
    /// </summary>
    public bool RequireDeposit { get; init; }

    /// <summary>
    /// Kapora tutarı (RequireDeposit=true ise).
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Gösterilecek mesaj.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
