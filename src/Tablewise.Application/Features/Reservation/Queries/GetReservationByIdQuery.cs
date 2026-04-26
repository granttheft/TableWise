using MediatR;
using Tablewise.Application.DTOs.Reservation;

namespace Tablewise.Application.Features.Reservation.Queries;

/// <summary>
/// ID ile rezervasyon detayı sorgusu.
/// </summary>
public sealed record GetReservationByIdQuery : IRequest<ReservationDto>
{
    /// <summary>
    /// Rezervasyon ID.
    /// </summary>
    public Guid ReservationId { get; init; }
}
