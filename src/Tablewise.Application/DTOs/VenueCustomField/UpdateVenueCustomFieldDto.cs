using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.VenueCustomField;

/// <summary>
/// Custom field güncelleme DTO'su.
/// </summary>
public sealed record UpdateVenueCustomFieldDto
{
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
    /// Select tipi için seçenekler (JSON array formatında).
    /// </summary>
    public string? Options { get; init; }
}
