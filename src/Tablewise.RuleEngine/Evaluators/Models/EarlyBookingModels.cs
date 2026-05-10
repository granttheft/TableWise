using Tablewise.RuleEngine.Base;

namespace Tablewise.RuleEngine.Evaluators.Models;

/// <summary>
/// Erken rezervasyon kuralı koşulları.
/// </summary>
public sealed class EarlyBookingConditions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Minimum kaç gün öncesinden rezervasyon yapılmalı.
    /// </summary>
    public int MinDaysInAdvance { get; init; }

    /// <summary>
    /// Maksimum kaç gün öncesinden rezervasyon yapılabilir (opsiyonel).
    /// </summary>
    public int? MaxDaysInAdvance { get; init; }
}

/// <summary>
/// Erken rezervasyon kuralı aksiyonları.
/// </summary>
public sealed class EarlyBookingActions : IVersionedJson
{
    /// <summary>
    /// Şema versiyonu.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Uygulanacak indirim yüzdesi.
    /// </summary>
    public decimal? DiscountPercent { get; init; }

    /// <summary>
    /// Kullanıcıya gösterilecek etiket/mesaj.
    /// </summary>
    public string? Label { get; init; }
}
