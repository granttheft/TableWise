using MediatR;

namespace Tablewise.Application.Features.TableCombination.Commands;

/// <summary>
/// Masa kombinasyonu oluşturma komutu.
/// </summary>
public sealed record CreateTableCombinationCommand : IRequest<Guid>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kombinasyon adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Birleştirilen masa ID'leri.
    /// </summary>
    public required List<Guid> TableIds { get; init; }

    /// <summary>
    /// Birleşik toplam kapasite (opsiyonel).
    /// </summary>
    public int? CombinedCapacity { get; init; }
}
