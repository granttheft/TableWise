using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Features.Platform.Auth;

namespace Tablewise.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/platform/auth")]
[Produces("application/json")]
public sealed class PlatformAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformAuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Platform çalışanı girişi. Başarılı olursa platform JWT döner.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(PlatformAuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PlatformAuthResultDto>> Login(
        [FromBody] PlatformLoginDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PlatformLoginCommand(dto.Email, dto.Password), cancellationToken);
        return Ok(result);
    }
}
