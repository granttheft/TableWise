using MediatR;
using Tablewise.Application.DTOs.Reservation;

namespace Tablewise.Application.Features.Reservation.Queries;

/// <summary>
/// Rezervasyon listesi sorgusu (staff/owner için).
/// </summary>
public sealed record GetReservationsQuery : IRequest<ReservationListResponseDto>
{
    /// <summary>
    /// Mekan ID filtresi.
    /// </summary>
    public Guid? VenueId { get; init; }

    /// <summary>
    /// Başlangıç tarihi.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Bitiş tarihi.
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// Durum filtresi (virgülle ayrılmış).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Arama terimi.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Masa ID filtresi.
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Sayfa numarası.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Sayfa boyutu.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Sıralama alanı.
    /// </summary>
    public string SortBy { get; init; } = "ReservedFor";

    /// <summary>
    /// Sıralama yönü.
    /// </summary>
    public string SortDirection { get; init; } = "asc";
}
