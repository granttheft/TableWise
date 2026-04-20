namespace Tablewise.Domain.Enums;

/// <summary>
/// Rezervasyon durumu. İş akışı (workflow) için kullanılır.
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Beklemede. Henüz onaylanmamış.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Onaylanmış. ConfirmCode oluşturuldu, müşteriye bildirim gönderildi.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Tamamlandı. Müşteri geldi ve hizmet verildi.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// İptal edildi. Müşteri veya mekan tarafından iptal edildi.
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Gelmedi (No-show). Rezervasyon saati geçti ancak müşteri gelmedi.
    /// </summary>
    NoShow = 4
}
