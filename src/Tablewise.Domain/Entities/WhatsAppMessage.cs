using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// WhatsApp mesaj kaydı. Her gönderim girişimi buraya loglanır.
/// ToPhone alanı GDPR/KVKK uyumu için maskelenmiş halde saklanır.
/// </summary>
public class WhatsAppMessage : TenantScopedEntity
{
    /// <summary>
    /// İlgili rezervasyon ID (opsiyonel).
    /// </summary>
    public Guid? ReservationId { get; set; }

    /// <summary>
    /// Alıcı telefon numarası — maskelenmiş: +905***9876
    /// </summary>
    public string ToPhone { get; set; } = string.Empty;

    /// <summary>
    /// Gönderilen şablon türü.
    /// </summary>
    public WhatsAppMessageTemplate Template { get; set; }

    /// <summary>
    /// Twilio'dan dönen mesaj SID (takip için).
    /// </summary>
    public string? ProviderMessageId { get; set; }

    /// <summary>
    /// Mevcut teslimat durumu.
    /// </summary>
    public WhatsAppMessageStatus Status { get; set; } = WhatsAppMessageStatus.Queued;

    /// <summary>
    /// Gönderim hatası varsa açıklaması.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Twilio'ya gönderildiği zaman (UTC).
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Alıcı telefonuna iletildiği zaman (UTC).
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// İlgili rezervasyon.
    /// </summary>
    public virtual Reservation? Reservation { get; set; }
}
