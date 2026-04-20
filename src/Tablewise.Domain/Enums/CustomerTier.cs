namespace Tablewise.Domain.Enums;

/// <summary>
/// Müşteri segmentasyonu için tier sistemi. Kural motoru ve CRM için kullanılır.
/// </summary>
public enum CustomerTier
{
    /// <summary>
    /// Normal müşteri. Özel öncelik yok.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Altın müşteri. Orta seviye öncelik.
    /// </summary>
    Gold = 1,

    /// <summary>
    /// VIP müşteri. Yüksek öncelik, özel muamele.
    /// </summary>
    VIP = 2,

    /// <summary>
    /// Kara listeye alınmış. Rezervasyon yapamaz.
    /// </summary>
    Blacklisted = 3
}
