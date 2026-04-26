using MediatR;

namespace Tablewise.Application.Features.Reservation.Queries;

/// <summary>
/// Rezervasyonları CSV olarak export etme sorgusu.
/// </summary>
public sealed record ExportReservationsQuery : IRequest<ExportReservationsResult>
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
    /// Durum filtresi.
    /// </summary>
    public string? Status { get; init; }
}

/// <summary>
/// Export sonucu.
/// </summary>
public sealed record ExportReservationsResult
{
    /// <summary>
    /// CSV içeriği.
    /// </summary>
    public byte[] Content { get; init; } = [];

    /// <summary>
    /// Dosya adı.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Content type.
    /// </summary>
    public string ContentType { get; init; } = "text/csv";
}
