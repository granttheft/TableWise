namespace Tablewise.Domain.Interfaces;

/// <summary>
/// Tenant context yönetimi. Global Query Filter için kullanılır.
/// HER istekte bir tenant belirlenmeli (SuperAdmin hariç).
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Aktif tenant ID. Nullable DEĞİL — her zaman set edilmiş olmalı.
    /// Set edilmemişse exception fırlatır.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Tenant context'i set eder. Middleware tarafından çağrılır.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    void SetTenant(Guid tenantId);
}
