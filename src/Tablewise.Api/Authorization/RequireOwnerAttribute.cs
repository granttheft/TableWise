using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Api.Authorization;

/// <summary>
/// Owner rolü gerektirir. Staff erişemez.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireOwnerAttribute : Attribute, IAuthorizationFilter
{
    /// <inheritdoc />
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var roleClaim = user.FindFirst("role")?.Value;
        
        if (!Enum.TryParse<UserRole>(roleClaim, out var role) || role != UserRole.Owner)
        {
            throw new ForbiddenException("Bu işlem için Owner yetkisi gereklidir.");
        }
    }
}
