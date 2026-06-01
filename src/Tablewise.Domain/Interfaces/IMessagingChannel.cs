using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Interfaces;

/// <summary>
/// Mesajlaşma kanalı soyutlaması. WhatsApp, SMS vb. kanallar bu interface'i implement eder.
/// </summary>
public interface IMessagingChannel
{
    /// <summary>
    /// Kanalın türü.
    /// </summary>
    MessagingChannelType ChannelType { get; }

    /// <summary>
    /// Şablon tabanlı mesaj gönderir.
    /// </summary>
    /// <param name="toPhone">Alıcı telefonu (E.164: +905...).</param>
    /// <param name="template">Kullanılacak şablon.</param>
    /// <param name="data">Şablon placeholder verileri (key: placeholder adı, value: değer).</param>
    /// <param name="reservationId">İlgili rezervasyon ID (log için, opsiyonel).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Sağlayıcı mesaj ID (Twilio SID). Başarısızsa null.</returns>
    Task<string?> SendTemplatedAsync(
        string toPhone,
        WhatsAppMessageTemplate template,
        Dictionary<string, string> data,
        Guid? reservationId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Serbest metin mesajı gönderir.
    /// </summary>
    /// <param name="toPhone">Alıcı telefonu (E.164).</param>
    /// <param name="body">Mesaj içeriği.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Sağlayıcı mesaj ID. Başarısızsa null.</returns>
    Task<string?> SendTextAsync(
        string toPhone,
        string body,
        CancellationToken ct = default);

    /// <summary>
    /// Sağlayıcıdan teslimat durumunu sorgular.
    /// </summary>
    /// <param name="providerMessageId">Sağlayıcı mesaj ID (Twilio SID).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Mevcut durum. Bulunamazsa null.</returns>
    Task<WhatsAppMessageStatus?> GetDeliveryStatusAsync(
        string providerMessageId,
        CancellationToken ct = default);
}
