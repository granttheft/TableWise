using Tablewise.Domain.Entities;

namespace Tablewise.Application.Interfaces;

/// <summary>
/// Platform kullanıcıları için JWT token servisi.
/// Mekan sahiplerinin JWT'sinden (IJwtTokenService) tamamen ayrı — farklı audience/issuer.
/// </summary>
public interface IPlatformJwtTokenService
{
    string GenerateToken(PlatformUser user);
}
