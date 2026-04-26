using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Table;

/// <summary>
/// Masa güncelleme DTO'su.
/// </summary>
public sealed record UpdateTableDto
{
    /// <summary>
    /// Masa adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Kapasite (1-50 arası).
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
