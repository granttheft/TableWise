using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Auth;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Api.Middleware;

/// <summary>
/// Tenant çözümleme middleware. Her istek için tenant context'i belirler.
/// JWT claim, URL slug veya bypass kurallarına göre çalışır.
/// </summary>
public sealed class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolverMiddleware> _logger;

    /// <summary>
    /// Tenant çözümleme gerektirmeyen path'ler.
    /// </summary>
    private static readonly string[] BypassPaths =
    [
        "/api/v1/auth/register",
        "/api/v1/auth/login",
        "/api/v1/auth/refresh",
        "/api/v1/auth/verify-email",
        "/api/v1/auth/forgot-password",
        "/api/v1/auth/reset-password",
        "/health",
        "/healthz",
        "/ready",
        "/metrics",
        "/swagger",
        "/openapi"
    ];

    /// <summary>
    /// Booking UI path prefix (slug'dan tenant çözümlenecek).
    /// </summary>
    private const string BookingPathPrefix = "/rezervasyon/";

    /// <summary>
    /// API booking path prefix (slug'dan tenant çözümlenecek).
    /// </summary>
    private const string ApiBookingPathPrefix = "/api/v1/book/";

    /// <summary>
    /// TenantResolverMiddleware constructor.
    /// </summary>
    public TenantResolverMiddleware(
        RequestDelegate next,
        ILogger<TenantResolverMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Middleware invoke metodu.
    /// </summary>
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        TablewiseDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Bypass path kontrolü
        if (ShouldBypass(path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        // Booking UI path kontrolü (/rezervasyon/{slug})
        if (path.StartsWith(BookingPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await ResolveFromSlugAsync(context, tenantContext, dbContext, path, BookingPathPrefix).ConfigureAwait(false);
            await _next(context).ConfigureAwait(false);
            return;
        }

        // API Booking path kontrolü (/api/v1/book/{slug}/...)
        if (path.StartsWith(ApiBookingPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // /api/v1/book/confirm/{code} gibi path'ler için slug yok - bypass
            var pathAfterBook = path[ApiBookingPathPrefix.Length..];
            if (pathAfterBook.StartsWith("confirm", StringComparison.OrdinalIgnoreCase))
            {
                // ConfirmCode ile erişim - tenant filter'ı query içinde bypass edilecek
                await _next(context).ConfigureAwait(false);
                return;
            }

            await ResolveFromSlugAsync(context, tenantContext, dbContext, path, ApiBookingPathPrefix).ConfigureAwait(false);
            await _next(context).ConfigureAwait(false);
            return;
        }

        // Authenticated istek kontrolü
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // SuperAdmin bypass
            var roleClaim = context.User.FindFirstValue(CustomClaimTypes.Role);
            if (roleClaim == UserRole.SuperAdmin.ToString())
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            // JWT'den tenant_id çöz
            var tenantIdClaim = context.User.FindFirstValue(CustomClaimTypes.TenantId);
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                tenantContext.SetTenant(tenantId);
                await _next(context).ConfigureAwait(false);
                return;
            }

            _logger.LogWarning(
                "JWT'de tenant_id claim bulunamadı. UserId: {UserId}",
                context.User.FindFirstValue(ClaimTypes.NameIdentifier));

            throw new UnauthorizedException("Geçersiz oturum. Lütfen tekrar giriş yapın.");
        }

        // Unauthenticated ve bypass değil → 401
        throw new UnauthorizedException("Bu işlem için giriş yapmanız gerekiyor.");
    }

    /// <summary>
    /// URL slug'dan tenant çözümler (booking UI için).
    /// </summary>
    private async Task ResolveFromSlugAsync(
        HttpContext context,
        ITenantContext tenantContext,
        TablewiseDbContext dbContext,
        string path,
        string prefix)
    {
        // /{prefix}/{slug}/... formatından slug çıkar
        var pathAfterPrefix = path[prefix.Length..];
        var slashIndex = pathAfterPrefix.IndexOf('/');
        var slug = slashIndex > 0 ? pathAfterPrefix[..slashIndex] : pathAfterPrefix;

        if (string.IsNullOrEmpty(slug))
        {
            throw new NotFoundException("Tenant", slug, "Mekan bulunamadı.");
        }

        // Slug'dan tenant ID'yi al (cache'lenebilir - ileride Redis'e taşınabilir)
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Slug == slug && !t.IsDeleted && t.IsActive)
            .Select(t => new { t.Id, t.PlanStatus })
            .FirstOrDefaultAsync(context.RequestAborted)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new NotFoundException("Tenant", slug, "Mekan bulunamadı.");
        }

        // Suspended/Cancelled tenant kontrolü
        if (tenant.PlanStatus is PlanStatus.Suspended or PlanStatus.Cancelled)
        {
            throw new ForbiddenException("Bu mekanın rezervasyon sistemi geçici olarak devre dışı.");
        }

        tenantContext.SetTenant(tenant.Id);

        // Slug'ı route value olarak ekle (controller'da kullanılabilir)
        context.Items["TenantSlug"] = slug;
    }

    /// <summary>
    /// Bypass edilmesi gereken path mi kontrol eder.
    /// </summary>
    private static bool ShouldBypass(string path)
    {
        foreach (var bypassPath in BypassPaths)
        {
            if (path.StartsWith(bypassPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
