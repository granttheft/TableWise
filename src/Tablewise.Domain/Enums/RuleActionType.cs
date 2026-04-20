namespace Tablewise.Domain.Enums;

/// <summary>
/// Kural motoru aksiyonu. RuleTemplate ve RuleExecution için kullanılır.
/// </summary>
public enum RuleActionType
{
    /// <summary>
    /// İzin ver (allow). Rezervasyonu onaylar.
    /// </summary>
    Allow = 0,

    /// <summary>
    /// Engelle (block). Rezervasyonu reddet.
    /// </summary>
    Block = 1,

    /// <summary>
    /// Uyarı göster (warn). Kullanıcıya uyarı mesajı gösterir ama engelleme yok.
    /// </summary>
    Warn = 2,

    /// <summary>
    /// Öneri sun (suggest). Alternatif slot/masa öner.
    /// </summary>
    Suggest = 3,

    /// <summary>
    /// İndirim uygula (discount). Fiyat üzerinde indirim.
    /// </summary>
    Discount = 4,

    /// <summary>
    /// Kapora talep et (deposit). Pro+ planlarda.
    /// </summary>
    Deposit = 5,

    /// <summary>
    /// Yönlendir (redirect). Başka bir venue veya slot'a yönlendir.
    /// </summary>
    Redirect = 6
}
