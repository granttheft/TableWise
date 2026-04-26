using MediatR;

namespace Tablewise.Application.Features.Booking.Commands;

/// <summary>
/// Public rezervasyon iptal komutu (müşteri tarafından).
/// En az 24 saat öncesinden iptal yapılabilir.
/// </summary>
public sealed record CancelPublicReservationCommand : IRequest<bool>
{
    /// <summary>
    /// Onay kodu.
    /// </summary>
    public string ConfirmCode { get; init; } = string.Empty;

    /// <summary>
    /// İptal nedeni (opsiyonel).
    /// </summary>
    public string? Reason { get; init; }
}
