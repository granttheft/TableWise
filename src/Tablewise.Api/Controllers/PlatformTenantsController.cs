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
[Route("api/platform/tenants")]
[Produces("application/json")]
public sealed class PlatformTenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformTenantsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Tüm tenant'ları listeler (sayfalı, aranabilir, filtrelenebilir).
    /// </summary>
    [HttpGet]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing, PlatformRole.Finance)]
    [ProducesResponseType(typeof(PagedResult<TenantSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TenantSummaryDto>>> GetTenants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] PlanStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTenantsQuery(page, pageSize, search, status), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tenant detayını döner (plan, kullanıcılar, mekanlar, iç notlar).
    /// </summary>
    [HttpGet("{tenantId:guid}")]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing, PlatformRole.Finance)]
    [ProducesResponseType(typeof(TenantDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDetailDto>> GetTenantDetail(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTenantDetailQuery(tenantId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tenant planını değiştirir. SuperAdmin ve Finance rolü gerektirir.
    /// </summary>
    [HttpPut("{tenantId:guid}/plan")]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Finance)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlan(
        Guid tenantId,
        [FromBody] UpdatePlanRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateTenantPlanCommand(tenantId, request.PlanId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Tenant'ı askıya alır veya aktif eder. Yalnızca SuperAdmin.
    /// </summary>
    [HttpPut("{tenantId:guid}/suspend")]
    [RequirePlatformRole(PlatformRole.SuperAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(
        Guid tenantId,
        [FromBody] SuspendRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new SuspendTenantCommand(tenantId, request.Suspend), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Tenant'a iç not ekler.
    /// </summary>
    [HttpPost("{tenantId:guid}/notes")]
    [RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Marketing, PlatformRole.Finance)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddNote(
        Guid tenantId,
        [FromBody] AddNoteRequest request,
        CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? "unknown";
        await _mediator.Send(new AddPlatformNoteCommand(tenantId, request.Content, email), cancellationToken);
        return NoContent();
    }
}

public record UpdatePlanRequest(Guid PlanId);
public record SuspendRequest(bool Suspend);
public record AddNoteRequest(string Content);
