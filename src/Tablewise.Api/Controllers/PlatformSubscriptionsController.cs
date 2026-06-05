using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Platform")]
[RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Finance)]
[Route("api/platform/subscriptions")]
[Produces("application/json")]
public sealed class PlatformSubscriptionsController : ControllerBase
{
    /// <summary>
    /// Ödeme takibi — Faz 7 (İyzico) tamamlandıktan sonra aktif olacak.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSubscriptions()
    {
        return Ok(new { message = "Bu özellik Faz 7 (İyzico entegrasyonu) tamamlandıktan sonra aktif olacak." });
    }
}
