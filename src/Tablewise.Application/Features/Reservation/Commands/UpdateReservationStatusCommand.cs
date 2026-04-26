using MediatR;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// Rezervasyon durumu güncelleme komutu.
/// </summary>
public sealed record UpdateReservationStatusCommand : IRequest<bool>
{
    /// <summary>
    /// Rezervasyon ID.
    /// </summary>
    public Guid ReservationId { get; init; }

    /// <summary>
    /// Yeni durum.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Değişiklik nedeni.
    /// </summary>
    public string? Reason { get; init; }
}
