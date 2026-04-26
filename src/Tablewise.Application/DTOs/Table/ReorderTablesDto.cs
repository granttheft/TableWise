namespace Tablewise.Application.DTOs.Table;

/// <summary>
/// Masa sıralama güncelleme DTO'su.
/// </summary>
public sealed record ReorderTablesDto
{
    /// <summary>
    /// Sıralama listesi (maksimum 100 adet).
    /// </summary>
    public required List<TableOrderItem> Items { get; init; }
}

/// <summary>
/// Sıralama item.
/// </summary>
public sealed record TableOrderItem
{
    /// <summary>
    /// Masa ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Yeni sıralama değeri.
    /// </summary>
    public required int SortOrder { get; init; }
}
