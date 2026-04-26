namespace Tablewise.Application.DTOs.TableCombination;

/// <summary>
/// Masa kombinasyonu DTO'su.
/// </summary>
public sealed record TableCombinationDto
{
    /// <summary>
    /// Kombinasyon ID.
    /// </summary>
    public required Guid Id { get; init; }

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
    /// Birleşik toplam kapasite.
    /// </summary>
    public required int CombinedCapacity { get; init; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
