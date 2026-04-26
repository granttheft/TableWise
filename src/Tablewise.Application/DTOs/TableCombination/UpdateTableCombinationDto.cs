namespace Tablewise.Application.DTOs.TableCombination;

/// <summary>
/// Masa kombinasyonu güncelleme DTO'su.
/// </summary>
public sealed record UpdateTableCombinationDto
{
    /// <summary>
    /// Kombinasyon adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Birleştirilen masa ID'leri (minimum 2 adet).
    /// </summary>
    public required List<Guid> TableIds { get; init; }

    /// <summary>
    /// Birleşik toplam kapasite.
    /// </summary>
    public required int CombinedCapacity { get; init; }
}
