using Tablewise.Domain.Common;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Idempotency key entity. POST /reserve gibi kritik endpoint'lerde duplicate request'leri önler.
/// Client tarafından üretilen unique key ile aynı isteğin birden fazla işlenmesini engeller.
/// Redis + DB ile cache'lenir.
/// </summary>
public class IdempotencyKey : TenantScopedEntity
{
    /// <summary>
    /// Idempotency key (client tarafından üretilir). Benzersiz olmalı.
    /// Header: Idempotency-Key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Cache'lenen response (JSONB). Aynı key ile tekrar istek gelirse bu response dönülür.
    /// </summary>
    public string ResponseJson { get; set; } = "{}";

    /// <summary>
    /// Key son kullanma tarihi (UTC). Expired key'ler temizlenir (background job).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Idempotency key'in ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }
}
