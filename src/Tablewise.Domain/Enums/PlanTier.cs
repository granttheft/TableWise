namespace Tablewise.Domain.Enums;

/// <summary>
/// Abonelik plan seviyesi. Feature flag'ler bu enum'a göre belirlenir.
/// </summary>
public enum PlanTier
{
    /// <summary>
    /// Başlangıç planı: 1 mekan, 3 masa, 5 kural, 100 rez/ay, Email.
    /// Fiyat: ₺490/ay
    /// </summary>
    Starter = 0,

    /// <summary>
    /// Pro plan: 1 mekan, sınırsız masa/kural, SMS, CRM tier, Kapora modülü.
    /// Fiyat: ₺990/ay
    /// </summary>
    Pro = 1,

    /// <summary>
    /// Business plan: 3 mekan, API erişimi, öncelikli destek.
    /// Fiyat: ₺1990/ay
    /// </summary>
    Business = 2,

    /// <summary>
    /// Enterprise plan: Sınırsız mekan, white-label, SLA, özel DB.
    /// Fiyat: Teklif
    /// </summary>
    Enterprise = 3
}
