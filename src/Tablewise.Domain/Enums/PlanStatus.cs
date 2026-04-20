namespace Tablewise.Domain.Enums;

/// <summary>
/// Abonelik durumu. Ödeme ve plan geçiş süreçlerinde kullanılır.
/// </summary>
public enum PlanStatus
{
    /// <summary>
    /// Deneme sürümü. Sınırlı süre ücretsiz erişim.
    /// </summary>
    Trial = 0,

    /// <summary>
    /// Aktif abonelik. Ödeme güncel.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Ödeme gecikmeli. Grace period içinde.
    /// </summary>
    PastDue = 2,

    /// <summary>
    /// Askıya alınmış. Ödeme yapılmadı, erişim kısıtlı.
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// İptal edilmiş. Abonelik sona ermiş.
    /// </summary>
    Cancelled = 4
}
