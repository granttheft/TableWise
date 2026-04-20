namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Tenant izolasyon ihlali. KRİTİK GÜVENLİK HATASI. Loglarda SECURITY tag ile işaretlenmeli.
/// HTTP 403 dönülür ve Sentry'e immediate gönderilir.
/// </summary>
public class TenantIsolationException : DomainException
{
    /// <summary>
    /// İstenen tenant ID.
    /// </summary>
    public Guid RequestedTenantId { get; }

    /// <summary>
    /// Aktif kullanıcının tenant ID'si.
    /// </summary>
    public Guid? CurrentTenantId { get; }

    /// <summary>
    /// TenantIsolationException constructor.
    /// </summary>
    /// <param name="requestedTenantId">İstenen tenant ID</param>
    /// <param name="currentTenantId">Aktif tenant ID</param>
    public TenantIsolationException(Guid requestedTenantId, Guid? currentTenantId)
        : base("Tenant isolation violation detected. This incident has been logged.", "TENANT_ISOLATION_VIOLATION")
    {
        RequestedTenantId = requestedTenantId;
        CurrentTenantId = currentTenantId;
    }

    /// <summary>
    /// TenantIsolationException constructor with custom message.
    /// </summary>
    /// <param name="requestedTenantId">İstenen tenant ID</param>
    /// <param name="currentTenantId">Aktif tenant ID</param>
    /// <param name="message">Özel hata mesajı</param>
    public TenantIsolationException(Guid requestedTenantId, Guid? currentTenantId, string message)
        : base(message, "TENANT_ISOLATION_VIOLATION")
    {
        RequestedTenantId = requestedTenantId;
        CurrentTenantId = currentTenantId;
    }
}
