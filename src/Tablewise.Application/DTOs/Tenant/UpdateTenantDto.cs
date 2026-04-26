namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Tenant güncelleme DTO'su.
/// </summary>
public sealed record UpdateTenantDto
{
    /// <summary>
    /// İşletme adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Özel ayarlar (JSON).
    /// </summary>
    public string? Settings { get; init; }
}
