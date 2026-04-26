using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Table;

/// <summary>
/// Masa DTO'su.
/// </summary>
public sealed record TableDto
{
    /// <summary>
    /// Masa ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Masa adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Kapasite (kişi sayısı).
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

    /// <summary>
    /// Sıralama önceliği.
    /// </summary>
    public required int SortOrder { get; init; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
