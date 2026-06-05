using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.Messaging;

/// <summary>
/// WhatsApp bildirim orkestratörü.
/// Venue.WhatsAppEnabled true ise WhatsApp, false ise email'e fallback yapar.
/// Ayrıca her bildirim tipi için venue seviyesinde açma/kapama desteği sunar.
/// </summary>
public sealed class WhatsAppOrchestrator : IWhatsAppOrchestrator
{
    private readonly IMessagingChannel _whatsApp;
    private readonly IEmailService _email;
    private readonly ILogger<WhatsAppOrchestrator> _logger;

    /// <summary>
    /// WhatsAppOrchestrator constructor.
    /// </summary>
    public WhatsAppOrchestrator(
        IMessagingChannel whatsApp,
        IEmailService email,
        ILogger<WhatsAppOrchestrator> logger)
    {
        _whatsApp = whatsApp;
        _email = email;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendReservationReceivedAsync(
        Reservation reservation,
        string venueName,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default)
    {
        if (venueWhatsAppEnabled && waNotify && !string.IsNullOrWhiteSpace(reservation.GuestPhone))
        {
            var data = BuildBaseData(reservation, venueName);
            await SafeSendAsync(() => _whatsApp.SendTemplatedAsync(
                reservation.GuestPhone,
                WhatsAppMessageTemplate.ReservationReceived,
                data,
                reservation.Id,
                ct), reservation.Id).ConfigureAwait(false);
        }
        else if (!string.IsNullOrWhiteSpace(reservation.GuestEmail))
        {
            await SafeEmailAsync(() => _email.SendReservationConfirmationAsync(
                reservation.GuestEmail!,
                reservation.GuestName,
                venueName,
                reservation.ReservedFor,
                reservation.ConfirmCode,
                ct), reservation.Id).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SendReservationConfirmedAsync(
        Reservation reservation,
        string venueName,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default)
    {
        if (venueWhatsAppEnabled && waNotify && !string.IsNullOrWhiteSpace(reservation.GuestPhone))
        {
            var data = BuildBaseData(reservation, venueName);
            await SafeSendAsync(() => _whatsApp.SendTemplatedAsync(
                reservation.GuestPhone,
                WhatsAppMessageTemplate.ReservationConfirmed,
                data,
                reservation.Id,
                ct), reservation.Id).ConfigureAwait(false);
        }
        else if (!string.IsNullOrWhiteSpace(reservation.GuestEmail))
        {
            await SafeEmailAsync(() => _email.SendReservationConfirmationAsync(
                reservation.GuestEmail!,
                reservation.GuestName,
                venueName,
                reservation.ReservedFor,
                reservation.ConfirmCode,
                ct), reservation.Id).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SendReminderAsync(
        Reservation reservation,
        string venueName,
        string? venueAddress,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default)
    {
        if (venueWhatsAppEnabled && waNotify && !string.IsNullOrWhiteSpace(reservation.GuestPhone))
        {
            var data = BuildBaseData(reservation, venueName);
            await SafeSendAsync(() => _whatsApp.SendTemplatedAsync(
                reservation.GuestPhone,
                WhatsAppMessageTemplate.Reminder,
                data,
                reservation.Id,
                ct), reservation.Id).ConfigureAwait(false);
        }
        else if (!string.IsNullOrWhiteSpace(reservation.GuestEmail))
        {
            await SafeEmailAsync(() => _email.SendReservationReminderAsync(
                reservation.GuestEmail!,
                reservation.GuestName,
                venueName,
                venueAddress,
                reservation.ReservedFor,
                reservation.ConfirmCode,
                ct), reservation.Id).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task SendCancellationAsync(
        Reservation reservation,
        string venueName,
        string bookingLink,
        bool venueWhatsAppEnabled,
        bool waNotify = true,
        CancellationToken ct = default)
    {
        if (venueWhatsAppEnabled && waNotify && !string.IsNullOrWhiteSpace(reservation.GuestPhone))
        {
            var data = BuildBaseData(reservation, venueName);
            data["iadeBilgisi"] = string.Empty; // Faz 7.5'te iade bilgisi eklenecek
            data["bookingLink"] = bookingLink;
            await SafeSendAsync(() => _whatsApp.SendTemplatedAsync(
                reservation.GuestPhone,
                WhatsAppMessageTemplate.Cancellation,
                data,
                reservation.Id,
                ct), reservation.Id).ConfigureAwait(false);
        }
        else if (!string.IsNullOrWhiteSpace(reservation.GuestEmail))
        {
            await SafeEmailAsync(() => _email.SendReservationCancellationAsync(
                reservation.GuestEmail!,
                reservation.GuestName,
                venueName,
                reservation.ReservedFor,
                reservation.ConfirmCode,
                ct), reservation.Id).ConfigureAwait(false);
        }
    }

    // ---- Private ----

    private static Dictionary<string, string> BuildBaseData(Reservation r, string venueName) => new()
    {
        ["ad"] = r.GuestName,
        ["mekan"] = venueName,
        ["tarih"] = r.ReservedFor.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
        ["saat"] = r.ReservedFor.ToString("HH:mm"),
        ["kisi"] = r.PartySize.ToString(),
    };

    private async Task SafeSendAsync(Func<Task<string?>> send, Guid reservationId)
    {
        try
        {
            await send().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp gönderilemedi. ReservationId: {ReservationId}", reservationId);
        }
    }

    private async Task SafeEmailAsync(Func<Task> send, Guid reservationId)
    {
        try
        {
            await send().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback email gönderilemedi. ReservationId: {ReservationId}", reservationId);
        }
    }
}
