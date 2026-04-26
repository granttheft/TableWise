using System.Text;
using System.Text.Json;
using Microsoft.IO;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Api.Middleware;

/// <summary>
/// Idempotency middleware. POST reserve endpoint'lerinde duplicate request'leri önler.
/// Idempotency-Key header zorunlu, aynı key ile gelen istek cache'den döner.
/// </summary>
public sealed class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;
    private readonly RecyclableMemoryStreamManager _streamManager;

    /// <summary>
    /// Idempotency gerektiren path'ler.
    /// </summary>
    private static readonly string[] RequiredPaths =
    [
        "/api/v1/book/",
        "/reserve"
    ];

    /// <summary>
    /// IdempotencyMiddleware constructor.
    /// </summary>
    public IdempotencyMiddleware(
        RequestDelegate next,
        ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _streamManager = new RecyclableMemoryStreamManager();
    }

    /// <summary>
    /// Middleware invoke.
    /// </summary>
    public async Task InvokeAsync(
        HttpContext context,
        IIdempotencyService idempotencyService,
        ITenantContext tenantContext)
    {
        // Sadece POST isteklerinde aktif
        if (context.Request.Method != HttpMethods.Post)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        // Idempotency gerektiren path mi?
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var requiresIdempotency = RequiredPaths.Any(p => path.Contains(p) && path.Contains("reserve"));

        if (!requiresIdempotency)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        // Idempotency-Key header kontrolü
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            _logger.LogWarning("Idempotency-Key header eksik. Path: {Path}", path);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "Idempotency-Key header required for reservation requests."
            };

            await context.Response.WriteAsJsonAsync(errorResponse).ConfigureAwait(false);
            return;
        }

        var key = idempotencyKey.ToString();
        Guid tenantId;

        try
        {
            tenantId = tenantContext.TenantId;
        }
        catch
        {
            // Tenant henüz resolve edilmemişse, public endpoint'te slug'dan alınacak
            // Bu durumda idempotency key'e slug ekleyerek unique yapıyoruz
            var slug = ExtractSlugFromPath(path);
            tenantId = Guid.Empty; // Placeholder - gerçek tenant ID olmadan da çalışabilir
            key = $"{slug}:{key}";
        }

        // Cache kontrol
        var cachedResponse = await idempotencyService.GetAsync(tenantId, key).ConfigureAwait(false);

        if (cachedResponse != null)
        {
            _logger.LogInformation("Idempotency hit. Key: {Key}", key);

            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = cachedResponse.ContentType;

            if (cachedResponse.Headers != null)
            {
                foreach (var header in cachedResponse.Headers)
                {
                    context.Response.Headers.TryAdd(header.Key, header.Value);
                }
            }

            context.Response.Headers.Append("X-Idempotency-Replay", "true");

            await context.Response.WriteAsync(cachedResponse.Body).ConfigureAwait(false);
            return;
        }

        // Response'u capture etmek için body'yi intercept et
        var originalBodyStream = context.Response.Body;

        await using var responseBody = _streamManager.GetStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context).ConfigureAwait(false);

            // Response'u oku
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync().ConfigureAwait(false);

            // Başarılı response'u cache'le (2xx status kodları)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                var responseToCache = new CachedIdempotencyResponse
                {
                    StatusCode = context.Response.StatusCode,
                    Body = responseContent,
                    ContentType = context.Response.ContentType ?? "application/json",
                    CreatedAt = DateTime.UtcNow
                };

                // Fire-and-forget olarak cache'e yaz
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await idempotencyService.SaveAsync(tenantId, key, responseToCache).ConfigureAwait(false);
                        _logger.LogDebug("Idempotency response cached. Key: {Key}", key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Idempotency cache yazılamadı. Key: {Key}", key);
                    }
                });
            }

            // Response'u orijinal stream'e kopyala
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream).ConfigureAwait(false);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    /// <summary>
    /// Path'ten slug çıkarır.
    /// </summary>
    private static string ExtractSlugFromPath(string path)
    {
        // /api/v1/book/{slug}/reserve
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Equals("book", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length)
            {
                return parts[i + 1];
            }
        }
        return "unknown";
    }
}
