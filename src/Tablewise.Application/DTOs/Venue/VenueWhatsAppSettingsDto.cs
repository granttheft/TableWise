namespace Tablewise.Application.DTOs.Venue;

/// <summary>
/// Venue WhatsApp bildirim ayarları DTO'su.
/// </summary>
public sealed record VenueWhatsAppSettingsDto
{
    /// <summary>
    /// WhatsApp bildirimleri aktif mi?
    /// </summary>
    public required bool WhatsAppEnabled { get; init; }

    /// <summary>
    /// Rezervasyon alındı bildirimi aktif mi?
    /// </summary>
    public required bool NotifyReservationReceived { get; init; }

    /// <summary>
    /// Rezervasyon onay bildirimi aktif mi?
    /// </summary>
    public required bool NotifyReservationConfirmed { get; init; }

    /// <summary>
    /// Hatırlatma bildirimi aktif mi?
    /// </summary>
    public required bool NotifyReminder { get; init; }

    /// <summary>
    /// İptal bildirimi aktif mi?
    /// </summary>
    public required bool NotifyCancellation { get; init; }

    /// <summary>
    /// Platform Twilio bağlantısı yapılandırılmış mı? (AccountSid dolu mu)
    /// </summary>
    public required bool IsConnected { get; init; }
}
