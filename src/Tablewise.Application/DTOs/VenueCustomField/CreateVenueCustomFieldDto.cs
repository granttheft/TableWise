using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.VenueCustomField;

/// <summary>
/// Custom field oluşturma DTO'su.
/// </summary>
public sealed record CreateVenueCustomFieldDto
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
    public bool IsRequired { get; init; } = false;

    /// <summary>
    /// Select tipi için seçenekler (JSON array formatında).
    /// Örnek: ["Seçenek 1", "Seçenek 2"]
    /// </summary>
    public string? Options { get; init; }
}
