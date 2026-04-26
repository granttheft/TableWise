using MediatR;

namespace Tablewise.Application.Features.VenueClosure.Commands;

/// <summary>
/// Toplu kapalılık oluşturma komutu.
/// Maksimum 50 adet kapalılık kaydı oluşturabilir.
/// </summary>
public sealed record BulkCreateVenueClosureCommand : IRequest<List<Guid>>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kapalılık listesi.
    /// </summary>
    public required List<CreateVenueClosureItem> Closures { get; init; }
}

/// <summary>
/// Toplu kapalılık oluşturma item.
/// </summary>
public sealed record CreateVenueClosureItem
{
    /// <summary>
    /// Başlangıç tarihi.
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// Bitiş tarihi.
    /// </summary>
    public required DateTime EndDate { get; init; }

    /// <summary>
    /// Tüm gün kapalı mı?
    /// </summary>
    public bool IsFullDay { get; init; } = true;

    /// <summary>
    /// Açılış saati.
    /// </summary>
    public TimeSpan? OpenTime { get; init; }

    /// <summary>
    /// Kapanış saati.
    /// </summary>
    public TimeSpan? CloseTime { get; init; }

    /// <summary>
    /// Neden.
    /// </summary>
    public string? Reason { get; init; }
}
