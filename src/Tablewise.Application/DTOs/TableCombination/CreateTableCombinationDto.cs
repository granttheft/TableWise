namespace Tablewise.Application.DTOs.TableCombination;

/// <summary>
/// Masa kombinasyonu oluşturma DTO'su.
/// </summary>
public sealed record CreateTableCombinationDto
{
    /// <summary>
    /// Kombinasyon adı (venue içinde unique olmalı).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Birleştirilen masa ID'leri (minimum 2 adet).
    /// </summary>
    public required List<Guid> TableIds { get; init; }

    /// <summary>
    /// Birleşik toplam kapasite.
    /// Null ise otomatik hesaplanır (seçili masaların toplamı).
    /// </summary>
    public int? CombinedCapacity { get; init; }
}
