using Tablewise.Domain.Entities;

namespace Tablewise.Application.Interfaces;

/// <summary>
/// WhatsApp bildirim orkestratörü. Venue ayarlarına göre WhatsApp veya email'e yönlendirir.
/// </summary>
public interface IWhatsAppOrchestrator
{
    /// <summary>
    /// Rezervasyon alındı bildirimi gönderir (kapora bekliyor veya kaporasız).
    /// </summary>
    Task SendReservationReceivedAsync(
        Reservation reservation,
        string venueName,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default);

    /// <summary>
    /// Rezervasyon onaylandı bildirimi gönderir.
    /// </summary>
    Task SendReservationConfirmedAsync(
        Reservation reservation,
        string venueName,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default);

    /// <summary>
    /// Rezervasyon hatırlatma bildirimi gönderir (1 gün önce).
    /// </summary>
    Task SendReminderAsync(
        Reservation reservation,
        string venueName,
        string? venueAddress,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default);

    /// <summary>
    /// Rezervasyon iptal bildirimi gönderir.
    /// </summary>
    Task SendCancellationAsync(
        Reservation reservation,
        string venueName,
        string bookingLink,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default);
}
