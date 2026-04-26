using MediatR;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Features.Booking.Commands;

/// <summary>
/// Rezervasyon değiştirme komutu.
/// En az 24 saat öncesinden değişiklik yapılabilir.
/// </summary>
public sealed record ModifyReservationCommand : IRequest<ReserveResponseDto>
{
    /// <summary>
    /// Onay kodu.
    /// </summary>
    public string ConfirmCode { get; init; } = string.Empty;

    /// <summary>
    /// Yeni tarih/saat (opsiyonel).
    /// </summary>
    public DateTime? NewDateTime { get; init; }

    /// <summary>
    /// Yeni masa ID (opsiyonel).
    /// </summary>
    public Guid? NewTableId { get; init; }

    /// <summary>
    /// Yeni kişi sayısı (opsiyonel).
    /// </summary>
    public int? NewPartySize { get; init; }

    /// <summary>
    /// Idempotency key.
    /// </summary>
    public string IdempotencyKey { get; init; } = string.Empty;
}
