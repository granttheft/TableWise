using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Bildirim log entity. Gönderilen email/SMS/push bildirimlerini kaydeder.
/// KVKK uyumu için alıcı bilgisi maskelenebilir.
/// </summary>
public class NotificationLog : TenantScopedEntity
{
    /// <summary>
    /// Bildirim hangi rezervasyonla ilgili (Foreign Key, nullable).
    /// Rezervasyon dışı bildirimler için null olabilir.
    /// </summary>
    public Guid? ReservationId { get; set; }

    /// <summary>
    /// Bildirim kanalı (Email, Sms, Push).
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Bildirim tipi (Confirm, Reminder, Cancel, NoShow, Welcome, PasswordReset).
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Alıcı (email veya telefon). KVKK için maskelenebilir (örn: "has***@gmail.com").
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Bildirim durumu. Enum string olarak (Sent, Failed, Pending).
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Hata mesajı (Status=Failed ise).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gönderilme tarihi (UTC). Null ise henüz gönderilmemiş.
    /// </summary>
    public DateTime? SentAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Bildirimin ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Bildirimin ilgili olduğu rezervasyon.
    /// </summary>
    public virtual Reservation? Reservation { get; set; }
}
