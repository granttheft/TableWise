using MediatR;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Features.Booking.Queries;

/// <summary>
/// Onay kodu ile rezervasyon detaylarını getiren sorgu.
/// </summary>
public sealed record GetReservationByCodeQuery : IRequest<ReservationDetailDto>
{
    /// <summary>
    /// Onay kodu (8 karakter).
    /// </summary>
    public string ConfirmCode { get; init; } = string.Empty;
}
