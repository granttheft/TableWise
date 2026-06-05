using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;

namespace Tablewise.Infrastructure.Auth;

/// <summary>
/// Platform çalışanları için JWT token üretici.
/// Audience/Issuer = "tablewise-platform" — mekan JWT'si ile karıştırılamaz.
/// </summary>
public sealed class PlatformJwtTokenService : IPlatformJwtTokenService
{
    private const string PlatformAudience = "tablewise-platform";
    private const string PlatformIssuer = "tablewise-platform";
    private const int ExpirationMinutes = 60;

    private readonly SymmetricSecurityKey _signingKey;

    public PlatformJwtTokenService(IOptions<JwtSettings> settings)
    {
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.SecretKey));
    }

    /// <inheritdoc />
    public string GenerateToken(PlatformUser user)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Name, user.FullName),
            new("platform_role", user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddMinutes(ExpirationMinutes),
            Issuer = PlatformIssuer,
            Audience = PlatformAudience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }
}
