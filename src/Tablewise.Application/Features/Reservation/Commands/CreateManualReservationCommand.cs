using MediatR;
using Tablewise.Application.DTOs.Reservation;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// Manuel rezervasyon oluşturma komutu (staff/owner için).
/// </summary>
public sealed record CreateManualReservationCommand : IRequest<ReservationDto>
{
    /// <summary>
    /// Mekan ID.
    /// </summary>
    public Guid VenueId { get; init; }

    /// <summary>
    /// Masa ID (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Masa birleşimi ID (opsiyonel).
    /// </summary>
    public Guid? TableCombinationId { get; init; }

    /// <summary>
    /// Misafir adı.
    /// </summary>
    public string GuestName { get; init; } = string.Empty;

    /// <summary>
    /// Misafir email.
    /// </summary>
    public string? GuestEmail { get; init; }

    /// <summary>
    /// Misafir telefon.
    /// </summary>
    public string GuestPhone { get; init; } = string.Empty;

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati.
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Özel istekler.
    /// </summary>
    public string? SpecialRequests { get; init; }

    /// <summary>
    /// Internal not.
    /// </summary>
    public string? InternalNotes { get; init; }

    /// <summary>
    /// Kapora bypass.
    /// </summary>
    public bool BypassDeposit { get; init; }

    /// <summary>
    /// Kural bypass.
    /// </summary>
    public bool BypassRules { get; init; }

    /// <summary>
    /// Onay email'i gönderilsin mi?
    /// </summary>
    public bool SendConfirmationEmail { get; init; } = true;
}
