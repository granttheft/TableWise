namespace Tablewise.Domain.Enums;

/// <summary>
/// Rezervasyon kaynağı. Rezervasyonun nereden geldiğini izlemek için kullanılır.
/// </summary>
public enum ReservationSource
{
    /// <summary>
    /// Booking UI üzerinden müşteri tarafından oluşturuldu.
    /// </summary>
    BookingUI = 0,

    /// <summary>
    /// Admin panelden Owner tarafından manuel oluşturuldu.
    /// </summary>
    ManualAdmin = 1,

    /// <summary>
    /// Admin panelden Staff tarafından manuel oluşturuldu.
    /// </summary>
    ManualStaff = 2,

    /// <summary>
    /// API üzerinden (Business+ plan) entegrasyon ile oluşturuldu.
    /// </summary>
    Api = 3,

    /// <summary>
    /// WhatsApp entegrasyonu üzerinden oluşturuldu (gelecek özellik).
    /// </summary>
    Whatsapp = 4
}
