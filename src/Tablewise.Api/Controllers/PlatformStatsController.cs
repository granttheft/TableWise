using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Features.Platform.Queries;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Platform")]
[RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing, PlatformRole.Finance)]
[Route("api/platform")]
[Produces("application/json")]
public sealed class PlatformStatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformStatsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Platform özet istatistikleri (tenant sayıları, MRR).
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(PlatformStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlatformStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlatformStatsQuery(), cancellationToken);
        return Ok(result);
    }
}
