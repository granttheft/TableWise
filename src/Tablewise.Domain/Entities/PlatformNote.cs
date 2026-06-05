using Tablewise.Domain.Common;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Platform çalışanlarının tenant'lara eklediği iç notlar.
/// TenantScopedEntity değil — platform-level erişimle oluşturulur.
/// </summary>
public class PlatformNote : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string CreatedByEmail { get; set; } = string.Empty;

    // Navigation
    public Tenant? Tenant { get; set; }
}
