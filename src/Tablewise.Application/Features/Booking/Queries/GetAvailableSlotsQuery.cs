using MediatR;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Features.Booking.Queries;

/// <summary>
/// Müsait slotları getiren sorgu.
/// </summary>
public sealed record GetAvailableSlotsQuery : IRequest<AvailabilityResponseDto>
{
    /// <summary>
    /// Tenant slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Tarih.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Belirli bir masa (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }
}
