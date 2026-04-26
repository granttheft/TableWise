using MediatR;
using Tablewise.Application.DTOs.Table;

namespace Tablewise.Application.Features.Table.Queries;

/// <summary>
/// Venue masaları listesi sorgusu.
/// SortOrder'a göre sıralı döner.
/// </summary>
public sealed record GetTablesQuery : IRequest<List<TableDto>>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Sadece aktif masalar mı?
    /// </summary>
    public bool ActiveOnly { get; init; } = false;
}
