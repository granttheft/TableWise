using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tablewise.Application.DTOs.Auth;
using Tablewise.Application.DTOs.Staff;
using Tablewise.Application.Features.Staff.Commands;
using Tablewise.Application.Features.Staff.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Davet işlemleri controller'ı.
/// Public endpoint'ler - authentication gerektirmez.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class InviteController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InviteController> _logger;

    /// <summary>
    /// InviteController constructor.
    /// </summary>
    public InviteController(
        IMediator mediator,
        ILogger<InviteController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Davet bilgilerini getirir (önizleme).
    /// Token'ın geçerliliğini kontrol eder.
    /// </summary>
    /// <param name="token">Davet token'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Davet önizleme bilgileri</returns>
    /// <response code="200">Davet bilgileri getirildi</response>
    /// <response code="400">Geçersiz, süresi dolmuş veya kabul edilmiş davet</response>
    [HttpGet("{token}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InvitationPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvitationPreview(
        string token,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInvitationPreviewQuery { Token = token };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Daveti kabul eder ve kullanıcı oluşturur.
    /// Direkt giriş yapmış sayılır (JWT döner).
    /// </summary>
    /// <param name="token">Davet token'ı</param>
    /// <param name="dto">Kullanıcı bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Auth sonucu (token + kullanıcı bilgileri)</returns>
    /// <response code="200">Davet kabul edildi, kullanıcı oluşturuldu</response>
    /// <response code="400">Geçersiz veya süresi dolmuş davet</response>
    /// <response code="409">Email zaten kayıtlı</response>
    /// <response code="422">Doğrulama hatası</response>
    [HttpPost("{token}/accept")]
    [AllowAnonymous]
    [EnableRateLimiting("accept-invite")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AcceptInvitation(
        string token,
        [FromBody] AcceptInvitationDto dto,
        CancellationToken cancellationToken = default)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        var command = new AcceptInvitationCommand
        {
            Token = token,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Password = dto.Password,
            PhoneNumber = dto.PhoneNumber,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    #region Private Helpers

    /// <summary>
    /// İstemci IP adresini alır.
    /// </summary>
    private string? GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// User-Agent header'ını alır.
    /// </summary>
    private string? GetUserAgent()
    {
        return Request.Headers.UserAgent.FirstOrDefault();
    }

    #endregion
}
