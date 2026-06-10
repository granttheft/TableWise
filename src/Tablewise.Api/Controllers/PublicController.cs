using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Features.Platform.Queries;

namespace Tablewise.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
[Produces("application/json")]
public sealed class PublicController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicController(IMediator mediator) => _mediator = mediator;

    [HttpGet("pricing")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanPricingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanPricingDto>>> GetPublicPricing(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPricingPlansQuery(), cancellationToken);
        return Ok(result);
    }
}
