using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;

namespace Tablewise.Infrastructure.Auth;

/// <summary>
/// JWT token üretim ve doğrulama servisi implementation.
/// HS256 kullanır (Faz 9'da RS256'ya geçilecek).
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly TokenValidationParameters _validationParameters;

    /// <summary>
    /// JwtTokenService constructor.
    /// </summary>
    /// <param name="settings">JWT ayarları</param>
    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewSeconds)
        };
    }

    /// <inheritdoc />
    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, Tenant tenant, PlanTier planTier)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(CustomClaimTypes.TenantId, tenant.Id.ToString()),
            new(CustomClaimTypes.Role, user.Role.ToString()),
            new(CustomClaimTypes.PlanTier, planTier.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return (tokenString, expiresAt);
    }

    /// <inheritdoc />
    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(bool extendedExpiry = false)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var token = Convert.ToBase64String(randomBytes);
        var days = extendedExpiry
            ? _settings.ExtendedRefreshTokenExpirationDays
            : _settings.RefreshTokenExpirationDays;
        var expiresAt = DateTime.UtcNow.AddDays(days);

        return (token, expiresAt);
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public string? GetClaimValue(string token, string claimType)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Custom claim type sabitleri.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// Tenant ID claim.
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// User role claim.
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// Plan tier claim.
    /// </summary>
    public const string PlanTier = "plan_tier";
}
