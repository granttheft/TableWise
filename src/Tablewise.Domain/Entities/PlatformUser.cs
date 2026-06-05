using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Tablewise platform çalışanları. Mekan sahiplerinden (User) tamamen ayrı auth sistemi.
/// TenantScopedEntity değil — platform-level.
/// </summary>
public class PlatformUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public PlatformRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}
