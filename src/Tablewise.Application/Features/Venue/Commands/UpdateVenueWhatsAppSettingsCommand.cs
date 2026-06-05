using MediatR;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue WhatsApp bildirim ayarlarını günceller.
/// </summary>
public sealed record UpdateVenueWhatsAppSettingsCommand : IRequest<Unit>
{
    /// <summary>
    /// Ayarlanacak Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

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
}
