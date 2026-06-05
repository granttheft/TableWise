using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Common;
using Tablewise.Application.DTOs.Venue;
using Tablewise.Application.DTOs.WhatsApp;
using Tablewise.Application.Features.Venue.Commands;
using Tablewise.Application.Features.Venue.Queries;
using Tablewise.Application.Features.WhatsApp.Queries;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Controllers;

/// <summary>
/// WhatsApp bildirim ayarları ve mesaj geçmişi controller'ı.
/// </summary>
[ApiController]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class WhatsAppSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// WhatsAppSettingsController constructor.
    /// </summary>
    public WhatsAppSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Venue'nun WhatsApp bildirim ayarlarını getirir.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>WhatsApp ayarları</returns>
    /// <response code="200">Ayarlar getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpGet("api/v1/venues/{venueId:guid}/whatsapp")]
    [ProducesResponseType(typeof(VenueWhatsAppSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWhatsAppSettings(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetVenueWhatsAppSettingsQuery(venueId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Venue'nun WhatsApp bildirim ayarlarını günceller.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Güncel ayarlar</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>204 No Content</returns>
    /// <response code="204">Ayarlar güncellendi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpPut("api/v1/venues/{venueId:guid}/whatsapp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWhatsAppSettings(
        Guid venueId,
        [FromBody] UpdateVenueWhatsAppSettingsRequest dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateVenueWhatsAppSettingsCommand
        {
            VenueId = venueId,
            WhatsAppEnabled = dto.WhatsAppEnabled,
            NotifyReservationReceived = dto.NotifyReservationReceived,
            NotifyReservationConfirmed = dto.NotifyReservationConfirmed,
            NotifyReminder = dto.NotifyReminder,
            NotifyCancellation = dto.NotifyCancellation
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Belirtilen telefona WhatsApp test mesajı gönderir.
    /// Twilio yapılandırılmamışsa no-op (geliştirme ortamında güvenli).
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Test mesajı parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>204 No Content</returns>
    /// <response code="204">Test mesajı gönderildi (veya no-op)</response>
    /// <response code="400">Geçersiz telefon numarası</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpPost("api/v1/venues/{venueId:guid}/whatsapp/test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendTestMessage(
        Guid venueId,
        [FromBody] SendTestMessageRequest dto,
        CancellationToken cancellationToken = default)
    {
        var command = new SendWhatsAppTestMessageCommand
        {
            VenueId = venueId,
            ToPhone = dto.ToPhone
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Tenant'ın WhatsApp mesaj geçmişini sayfalı olarak getirir.
    /// </summary>
    /// <param name="page">Sayfa numarası (1 tabanlı)</param>
    /// <param name="pageSize">Sayfa boyutu (max 100)</param>
    /// <param name="status">Durum filtresi (opsiyonel)</param>
    /// <param name="from">Başlangıç tarihi (UTC, opsiyonel)</param>
    /// <param name="to">Bitiş tarihi (UTC, opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalı mesaj geçmişi</returns>
    /// <response code="200">Geçmiş getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpGet("api/v1/whatsapp/messages")]
    [ProducesResponseType(typeof(PagedResult<WhatsAppMessageHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMessageHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] WhatsAppMessageStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWhatsAppMessageHistoryQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

/// <summary>
/// WhatsApp ayar güncelleme request body.
/// </summary>
public sealed record UpdateVenueWhatsAppSettingsRequest
{
    /// <summary>WhatsApp bildirimleri aktif mi?</summary>
    public required bool WhatsAppEnabled { get; init; }

    /// <summary>Rezervasyon alındı bildirimi aktif mi?</summary>
    public required bool NotifyReservationReceived { get; init; }

    /// <summary>Rezervasyon onay bildirimi aktif mi?</summary>
    public required bool NotifyReservationConfirmed { get; init; }

    /// <summary>Hatırlatma bildirimi aktif mi?</summary>
    public required bool NotifyReminder { get; init; }

    /// <summary>İptal bildirimi aktif mi?</summary>
    public required bool NotifyCancellation { get; init; }
}

/// <summary>
/// Test mesajı gönderme request body.
/// </summary>
public sealed record SendTestMessageRequest
{
    /// <summary>
    /// Alıcı telefon numarası (E.164 formatı: +905XXXXXXXXX).
    /// </summary>
    public required string ToPhone { get; init; }
}
