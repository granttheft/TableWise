using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Authorization;

/// <summary>
/// Owner veya Staff rolü gerektirir.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireOwnerOrStaffAttribute : Attribute, IAuthorizationFilter
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
        
        if (!Enum.TryParse<UserRole>(roleClaim, out var role) || 
            (role != UserRole.Owner && role != UserRole.Staff))
        {
            context.Result = new ForbidResult();
        }
    }
}
