using System.IdentityModel.Tokens.Jwt;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Features.Platform.Commands;
using Tablewise.Application.Features.Platform.Queries;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Platform")]
[Route("api/platform/users")]
[Produces("application/json")]
public sealed class PlatformUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformUsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePlatformRole(PlatformRole.SuperAdmin)]
    [ProducesResponseType(typeof(List<PlatformUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PlatformUserDto>>> GetUsers(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPlatformUsersQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [RequirePlatformRole(PlatformRole.SuperAdmin)]
    [ProducesResponseType(typeof(PlatformUserDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PlatformUserDto>> InviteUser(
        [FromBody] InvitePlatformUserDto dto,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new InvitePlatformUserCommand(dto), ct);
        return CreatedAtAction(nameof(GetUsers), result);
    }

    [HttpPut("{userId:guid}/role")]
    [RequirePlatformRole(PlatformRole.SuperAdmin)]
    [ProducesResponseType(typeof(PlatformUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlatformUserDto>> UpdateRole(
        Guid userId,
        [FromBody] UpdatePlatformUserRoleDto dto,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdatePlatformUserRoleCommand(userId, dto.NewRole), ct);
        return Ok(result);
    }

    [HttpPut("{userId:guid}/toggle-active")]
    [RequirePlatformRole(PlatformRole.SuperAdmin)]
    [ProducesResponseType(typeof(PlatformUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlatformUserDto>> ToggleActive(
        Guid userId,
        CancellationToken ct)
    {
        var requestingUserId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var result = await _mediator.Send(new TogglePlatformUserActiveCommand(userId, requestingUserId), ct);
        return Ok(result);
    }
}
