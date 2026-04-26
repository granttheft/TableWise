using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.VenueCustomField;

/// <summary>
/// Venue custom field DTO'su.
/// </summary>
public sealed record VenueCustomFieldDto
{
    /// <summary>
    /// Custom field ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Alan etiketi (label).
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Alan tipi.
    /// </summary>
    public required CustomFieldType FieldType { get; init; }

    /// <summary>
    /// Alan zorunlu mu?
    /// </summary>
    public required bool IsRequired { get; init; }

    /// <summary>
    /// Sıralama önceliği.
    /// </summary>
    public required int SortOrder { get; init; }

    /// <summary>
    /// Select tipi için seçenekler (JSON array).
    /// </summary>
    public string? Options { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
