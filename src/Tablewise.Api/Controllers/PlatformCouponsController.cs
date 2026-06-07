using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Common;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Features.Platform.Commands;
using Tablewise.Application.Features.Platform.Queries;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = "Platform")]
[Route("api/platform/coupons")]
[Produces("application/json")]
public sealed class PlatformCouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformCouponsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing, PlatformRole.Finance)]
    [ProducesResponseType(typeof(PagedResult<CouponDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CouponDto>>> GetCoupons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCouponsQuery(page, pageSize, search, isActive), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing)]
    [ProducesResponseType(typeof(CouponDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CouponDto>> CreateCoupon(
        [FromBody] CreateCouponDto dto,
        CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? "unknown";
        var result = await _mediator.Send(new CreateCouponCommand(dto, email), cancellationToken);
        return CreatedAtAction(nameof(GetCoupons), result);
    }

    [HttpPut("{couponId:guid}/deactivate")]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid couponId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateCouponCommand(couponId), cancellationToken);
        return NoContent();
    }
}
