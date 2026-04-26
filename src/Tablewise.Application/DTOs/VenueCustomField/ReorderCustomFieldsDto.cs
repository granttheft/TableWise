namespace Tablewise.Application.DTOs.VenueCustomField;

/// <summary>
/// Custom field sıralama güncelleme DTO'su.
/// </summary>
public sealed record ReorderCustomFieldsDto
{
    /// <summary>
    /// Sıralama listesi.
    /// </summary>
    public required List<CustomFieldOrderItem> Items { get; init; }
}

/// <summary>
/// Sıralama item.
/// </summary>
public sealed record CustomFieldOrderItem
{
    /// <summary>
    /// Custom field ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Yeni sıralama değeri.
    /// </summary>
    public required int SortOrder { get; init; }
}
