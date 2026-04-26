using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Table;

/// <summary>
/// Masa oluşturma DTO'su.
/// </summary>
public sealed record CreateTableDto
{
    /// <summary>
    /// Masa adı (venue içinde unique olmalı).
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
    /// Açıklama (opsiyonel).
    /// </summary>
    public string? Description { get; init; }
}
