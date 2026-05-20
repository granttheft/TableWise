using MediatR;
using Tablewise.Application.DTOs.Reservation;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// Manuel rezervasyon oluşturmadan önce slot ve kural değerlendirmesi.
/// </summary>
public sealed record EvaluateManualReservationCommand : IRequest<EvaluateManualReservationResponseDto>
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
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati.
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Kayıtlı müşteri ID (opsiyonel).
    /// </summary>
    public Guid? CustomerId { get; init; }

    /// <summary>
    /// Misafir email (opsiyonel).
    /// </summary>
    public string? GuestEmail { get; init; }

    /// <summary>
    /// Misafir telefon (opsiyonel).
    /// </summary>
    public string? GuestPhone { get; init; }
}
