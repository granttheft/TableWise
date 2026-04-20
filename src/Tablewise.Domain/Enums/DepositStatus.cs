namespace Tablewise.Domain.Enums;

/// <summary>
/// Kapora durumu. DepositTransaction için kullanılır.
/// </summary>
public enum DepositStatus
{
    /// <summary>
    /// Kapora gerekli değil.
    /// </summary>
    NotRequired = 0,

    /// <summary>
    /// Ödeme bekleniyor.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Ödeme alındı.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// İade edildi (rezervasyon iptal, refund policy'ye göre).
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Kesildi (no-show veya refund policy'ye göre).
    /// </summary>
    Forfeited = 4,

    /// <summary>
    /// Ödeme başarısız.
    /// </summary>
    Failed = 5
}
