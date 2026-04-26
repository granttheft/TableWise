using MediatR;

namespace Tablewise.Application.Features.TableCombination.Commands;

/// <summary>
/// Masa kombinasyonu güncelleme komutu.
/// </summary>
public sealed record UpdateTableCombinationCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Kombinasyon ID.
    /// </summary>
    public required Guid CombinationId { get; init; }

    /// <summary>
    /// Kombinasyon adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Birleştirilen masa ID'leri.
    /// </summary>
    public required List<Guid> TableIds { get; init; }

    /// <summary>
    /// Birleşik toplam kapasite.
    /// </summary>
    public required int CombinedCapacity { get; init; }
}
