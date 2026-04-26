using MediatR;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Table.Commands;

/// <summary>
/// Masa oluşturma komutu.
/// Plan limitlerini kontrol eder.
/// </summary>
public sealed record CreateTableCommand : IRequest<Guid>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Masa adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Kapasite.
    /// </summary>
    public required int Capacity { get; init; }

    /// <summary>
    /// Lokasyon.
    /// </summary>
    public required TableLocation Location { get; init; }

    /// <summary>
    /// Açıklama.
    /// </summary>
    public string? Description { get; init; }
}
