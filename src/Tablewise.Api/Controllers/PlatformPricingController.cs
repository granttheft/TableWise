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
[Route("api/platform/pricing")]
[Produces("application/json")]
public sealed class PlatformPricingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformPricingController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing, PlatformRole.Finance)]
    [ProducesResponseType(typeof(IReadOnlyList<PlanPricingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanPricingDto>>> GetPricingPlans(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPricingPlansQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{planId:guid}")]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Finance)]
    [ProducesResponseType(typeof(PlanPricingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanPricingDto>> UpdatePricing(
        Guid planId,
        [FromBody] UpdatePlanPricingDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdatePlanPricingCommand(planId, dto), cancellationToken);
        return Ok(result);
    }
}
