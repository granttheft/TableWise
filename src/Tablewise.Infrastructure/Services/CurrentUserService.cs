using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Current user service implementation. HttpContext.User claim'lerinden kullanıcı bilgilerini parse eder.
/// Authorization ve audit log için kullanılır.
/// </summary>
public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// CurrentUserService constructor.
    /// </summary>
    /// <param name="httpContextAccessor">HttpContextAccessor</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value;
            return string.IsNullOrWhiteSpace(tenantIdClaim) ? null : Guid.Parse(tenantIdClaim);
        }
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrWhiteSpace(userIdClaim) ? null : Guid.Parse(userIdClaim);
        }
    }

    /// <inheritdoc />
    public UserRole? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return string.IsNullOrWhiteSpace(roleClaim) ? null : Enum.Parse<UserRole>(roleClaim);
        }
    }

    /// <inheritdoc />
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
}
