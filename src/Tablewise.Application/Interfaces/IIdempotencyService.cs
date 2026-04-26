namespace Tablewise.Application.Interfaces;

/// <summary>
/// Idempotency (tekrar isteklerin önlenmesi) servisi.
/// POST rezervasyon gibi kritik endpoint'lerde aynı isteğin birden fazla işlenmesini önler.
/// Redis + DB fallback ile çalışır.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Idempotency key için cache'lenmiş response'u getirir.
    /// Önce Redis, yoksa DB'den arar.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="key">Idempotency key (client tarafından üretilir)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Cache'lenmiş response veya null</returns>
    Task<CachedIdempotencyResponse?> GetAsync(
        Guid tenantId,
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Response'u Idempotency key ile birlikte kaydeder.
    /// Redis'e 60 sn TTL, DB'ye 24 saat TTL ile yazılır.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="key">Idempotency key</param>
    /// <param name="response">Cache'lenecek response</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SaveAsync(
        Guid tenantId,
        string key,
        CachedIdempotencyResponse response,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Redis'in erişilebilir olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Redis erişilebilir ise true</returns>
    Task<bool> IsRedisAvailableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache'lenmiş idempotency response.
/// </summary>
public sealed record CachedIdempotencyResponse
{
    /// <summary>
    /// HTTP status kodu.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Response body (JSON).
    /// </summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>
    /// Content-Type header.
    /// </summary>
    public string ContentType { get; init; } = "application/json";

    /// <summary>
    /// Response headers (opsiyonel).
    /// </summary>
    public Dictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Oluşturulma zamanı (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
