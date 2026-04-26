using MediatR;

namespace Tablewise.Application.Features.Table.Commands;

/// <summary>
/// Masa sıralama güncelleme komutu.
/// Maksimum 100 adet masa sıralanabilir.
/// </summary>
public sealed record ReorderTablesCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Sıralama listesi.
    /// </summary>
    public required List<TableOrder> Orders { get; init; }
}

/// <summary>
/// Sıralama bilgisi.
/// </summary>
public sealed record TableOrder
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
