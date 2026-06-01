namespace Tablewise.Domain.Enums;

/// <summary>
/// WhatsApp mesaj teslimat durumu.
/// </summary>
public enum WhatsAppMessageStatus
{
    /// <summary>
    /// Kuyrukta bekliyor.
    /// </summary>
    Queued = 0,

    /// <summary>
    /// Twilio'ya gönderildi.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Alıcı telefonuna iletildi.
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// Alıcı tarafından okundu.
    /// </summary>
    Read = 3,

    /// <summary>
    /// Gönderim başarısız.
    /// </summary>
    Failed = 4,
}
