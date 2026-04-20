using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Geçici CurrentUser implementation (migration için).
/// Gerçek implementation API katmanında HttpContext üzerinden gelecek.
/// </summary>
public class DesignTimeCurrentUser : ICurrentUser
{
    public Guid? TenantId => null;
    public Guid? UserId => null;
    public UserRole? Role => null;
    public string? Email => "system";
}

/// <summary>
/// Geçici TenantContext implementation (migration için).
/// Gerçek implementation API katmanında HttpContext üzerinden gelecek.
/// </summary>
public class DesignTimeTenantContext : ITenantContext
{
    private Guid _tenantId = Guid.Empty;

    public Guid TenantId => _tenantId;

    public void SetTenant(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}
