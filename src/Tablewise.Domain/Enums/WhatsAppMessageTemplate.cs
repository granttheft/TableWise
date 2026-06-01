namespace Tablewise.Domain.Enums;

/// <summary>
/// WhatsApp mesaj şablonu türleri.
/// </summary>
public enum WhatsAppMessageTemplate
{
    /// <summary>
    /// Rezervasyon alındı (kapora bekliyor veya kaporasız).
    /// </summary>
    ReservationReceived = 0,

    /// <summary>
    /// Rezervasyon onaylandı (ödeme başarılı veya kaporasız onay).
    /// </summary>
    ReservationConfirmed = 1,

    /// <summary>
    /// Rezervasyon hatırlatması (1 gün önce).
    /// </summary>
    Reminder = 2,

    /// <summary>
    /// Rezervasyon iptal edildi.
    /// </summary>
    Cancellation = 3,
}
