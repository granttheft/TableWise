using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Authorization;

/// <summary>
/// Platform JWT'si ve belirtilen rol(ler)i gerektirir.
/// Mekan JWT'si ile bu endpoint'lere erişilemez — audience kontrolü yapılır.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequirePlatformRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly PlatformRole[] _allowedRoles;

    public RequirePlatformRoleAttribute(params PlatformRole[] roles)
    {
        _allowedRoles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Platform JWT audience kontrolü
        var audience = user.FindFirst("aud")?.Value;
        if (audience != "tablewise-platform")
        {
            context.Result = new ForbidResult();
            return;
        }

        var roleClaim = user.FindFirst("platform_role")?.Value;
        if (!Enum.TryParse<PlatformRole>(roleClaim, out var role) || !_allowedRoles.Contains(role))
        {
            context.Result = new ForbidResult();
        }
    }
}
