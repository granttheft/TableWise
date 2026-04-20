using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Tenant context implementation. HttpContext üzerinden TenantId çözümler.
/// JWT claim, URL slug veya subdomain'den tenant bilgisi alır.
/// </summary>
public class TenantContext : ITenantContext
{
    private Guid? _tenantId;

    /// <inheritdoc />
    public Guid TenantId
    {
        get
        {
            if (!_tenantId.HasValue || _tenantId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "TenantId henüz set edilmedi. Middleware tenant context'i doğru şekilde ayarlamış olmalı.");
            }
            return _tenantId.Value;
        }
    }

    /// <inheritdoc />
    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId boş olamaz.", nameof(tenantId));
        }

        _tenantId = tenantId;
    }
}
