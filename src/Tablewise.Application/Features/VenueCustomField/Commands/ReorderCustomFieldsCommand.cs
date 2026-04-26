using MediatR;

namespace Tablewise.Application.Features.VenueCustomField.Commands;

/// <summary>
/// Custom field sıralama güncelleme komutu.
/// </summary>
public sealed record ReorderCustomFieldsCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Sıralama listesi.
    /// </summary>
    public required List<CustomFieldOrder> Orders { get; init; }
}

/// <summary>
/// Sıralama bilgisi.
/// </summary>
public sealed record CustomFieldOrder
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
