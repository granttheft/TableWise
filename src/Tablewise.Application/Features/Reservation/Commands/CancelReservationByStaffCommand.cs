using MediatR;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// İşletme tarafından rezervasyon iptal komutu.
/// </summary>
public sealed record CancelReservationByStaffCommand : IRequest<bool>
{
    /// <summary>
    /// Rezervasyon ID.
    /// </summary>
    public Guid ReservationId { get; init; }

    /// <summary>
    /// İptal nedeni.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Bildirim gönderilsin mi?
    /// </summary>
    public bool SendNotification { get; init; } = true;

    /// <summary>
    /// Kapora iadesi yapılsın mı?
    /// </summary>
    public bool RefundDeposit { get; init; } = true;
}
