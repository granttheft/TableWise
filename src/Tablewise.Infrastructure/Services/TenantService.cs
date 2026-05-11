using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Tablewise.Application.Interfaces;
using Tablewise.Infrastructure.Auth;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Tenant servis implementasyonu.
/// HTTP context'ten mevcut tenant bilgisini alır.
/// </summary>
public sealed class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Mevcut tenant ID'yi JWT claim'lerinden alır.
    /// </summary>
    public Guid GetCurrentTenantId()
    {
        var tenantIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(CustomClaimTypes.TenantId)?.Value;

        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            throw new UnauthorizedAccessException("Tenant ID bulunamadı. Lütfen giriş yapın.");
        }

        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Geçersiz Tenant ID.");
        }

        return tenantId;
    }

    /// <summary>
    /// Mevcut user ID'yi JWT claim'lerinden alır.
    /// </summary>
    public Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID bulunamadı. Lütfen giriş yapın.");
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Geçersiz User ID.");
        }

        return userId;
    }

    /// <summary>
    /// Mevcut kullanıcının rolünü JWT claim'lerinden alır.
    /// </summary>
    public string GetCurrentUserRole()
    {
        var roleClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(CustomClaimTypes.Role)?.Value
            ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            throw new UnauthorizedAccessException("Rol bilgisi bulunamadı. Lütfen giriş yapın.");
        }

        return roleClaim;
    }
}
