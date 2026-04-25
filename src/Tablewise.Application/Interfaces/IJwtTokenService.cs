using System.Security.Claims;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Interfaces;

/// <summary>
/// JWT token üretim ve doğrulama servisi.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Access token (JWT) üretir.
    /// </summary>
    /// <param name="user">Kullanıcı entity</param>
    /// <param name="tenant">Tenant entity</param>
    /// <param name="planTier">Aktif plan seviyesi</param>
    /// <returns>JWT access token ve son kullanma tarihi</returns>
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, Tenant tenant, PlanTier planTier);

    /// <summary>
    /// Refresh token (random) üretir.
    /// </summary>
    /// <returns>Refresh token ve son kullanma tarihi</returns>
    (string Token, DateTime ExpiresAt) GenerateRefreshToken(bool extendedExpiry = false);

    /// <summary>
    /// JWT token'ı doğrular ve claim'leri döner.
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>ClaimsPrincipal veya null (geçersiz ise)</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Token'dan belirli bir claim değerini alır.
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <param name="claimType">Claim tipi</param>
    /// <returns>Claim değeri veya null</returns>
    string? GetClaimValue(string token, string claimType);
}
