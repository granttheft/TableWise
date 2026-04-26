using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Staff;
using Tablewise.Application.Features.Staff.Commands;
using Tablewise.Application.Features.Staff.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Personel yönetimi controller'ı.
/// Sadece Owner rolü erişebilir.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StaffController> _logger;

    /// <summary>
    /// StaffController constructor.
    /// </summary>
    public StaffController(
        IMediator mediator,
        ILogger<StaffController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Personel listesini getirir.
    /// </summary>
    /// <param name="activeOnly">Sadece aktif kullanıcılar mı?</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Personel listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<StaffMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListStaff(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = new ListStaffQuery { ActiveOnly = activeOnly };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Davet listesini getirir.
    /// </summary>
    /// <param name="pendingOnly">Sadece bekleyen davetler mi?</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Davet listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    [HttpGet("invitations")]
    [ProducesResponseType(typeof(List<InvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListInvitations(
        [FromQuery] bool pendingOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = new ListInvitationsQuery { PendingOnly = pendingOnly };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Personel daveti gönderir.
    /// </summary>
    /// <param name="dto">Davet bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan davet ID'si</returns>
    /// <response code="201">Davet başarıyla gönderildi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="409">Email zaten kayıtlı veya aktif davet var</response>
    [HttpPost("invitations")]
    [EnableRateLimiting("staff-invite")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InviteStaff(
        [FromBody] InviteStaffDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new InviteStaffCommand
        {
            Email = dto.Email,
            Role = dto.Role,
            Message = dto.Message
        };

        var invitationId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(InviteStaff), new { id = invitationId }, invitationId);
    }

    /// <summary>
    /// Daveti tekrar gönderir.
    /// </summary>
    /// <param name="id">Davet ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Davet tekrar gönderildi</response>
    /// <response code="404">Davet bulunamadı</response>
    [HttpPost("invitations/{id}/resend")]
    [EnableRateLimiting("staff-invite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendInvitation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ResendInvitationCommand { InvitationId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Daveti iptal eder.
    /// </summary>
    /// <param name="id">Davet ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Davet iptal edildi</response>
    /// <response code="404">Davet bulunamadı</response>
    [HttpDelete("invitations/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvitation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelInvitationCommand { InvitationId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Personel rolünü günceller.
    /// </summary>
    /// <param name="id">Kullanıcı ID'si</param>
    /// <param name="dto">Yeni rol</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Rol güncellendi</response>
    /// <response code="400">Geçersiz rol veya son Owner</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    [HttpPut("{id}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStaffRole(
        Guid id,
        [FromBody] UpdateStaffRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateStaffRoleCommand
        {
            UserId = id,
            NewRole = dto.Role
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Personeli siler (soft delete).
    /// </summary>
    /// <param name="id">Kullanıcı ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Personel silindi</response>
    /// <response code="400">Kendini veya son Owner'ı silme</response>
    /// <response code="404">Kullanıcı bulunamadı</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveStaff(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveStaffCommand { UserId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
