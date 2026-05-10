namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kural sıralama güncelleme DTO.
/// </summary>
public sealed record ReorderRulesDto
{
    /// <summary>
    /// Güncellenecek kural listesi.
    /// </summary>
    public List<RuleOrderItem> Rules { get; init; } = [];
}

/// <summary>
/// Kural sıralama item.
/// </summary>
public sealed record RuleOrderItem
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Yeni öncelik değeri.
    /// </summary>
    public int Priority { get; init; }
}
