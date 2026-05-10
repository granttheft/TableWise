namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Tenant venue DTO'su (basitleştirilmiş)
/// </summary>
public sealed class TenantVenueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TableCount { get; set; }
}
