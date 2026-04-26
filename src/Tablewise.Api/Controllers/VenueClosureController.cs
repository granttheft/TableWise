using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.VenueClosure;
using Tablewise.Application.Features.VenueClosure.Commands;
using Tablewise.Application.Features.VenueClosure.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Venue kapalılık yönetimi controller'ı.
/// Sadece Owner rolü erişebilir.
/// </summary>
[ApiController]
[Route("api/v1/venues/{venueId:guid}/closures")]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class VenueClosureController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VenueClosureController> _logger;

    /// <summary>
    /// VenueClosureController constructor.
    /// </summary>
    public VenueClosureController(
        IMediator mediator,
        ILogger<VenueClosureController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Venue'nin kapalı günlerini getirir (yıllık).
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kapalılık listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<VenueClosureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClosures(
        Guid venueId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVenueClosuresQuery
        {
            VenueId = venueId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni kapalı gün ekler.
    /// StartDate ile EndDate arası her gün için kapalılık kaydı oluşturur.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Kapalılık bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kapalılık ID'leri</returns>
    /// <response code="201">Kapalılık başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya çakışan kapalılık var</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpPost]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateClosure(
        Guid venueId,
        [FromBody] CreateVenueClosureDto dto,
        CancellationToken cancellationToken = default)
    {
        // Time string'i TimeSpan'e çevir
        TimeSpan? openTime = null;
        TimeSpan? closeTime = null;

        if (!dto.IsFullDay && !string.IsNullOrEmpty(dto.OpenTime))
        {
            TimeSpan.TryParseExact(dto.OpenTime, @"hh\:mm", null, out var parsed);
            openTime = parsed;
        }

        if (!dto.IsFullDay && !string.IsNullOrEmpty(dto.CloseTime))
        {
            TimeSpan.TryParseExact(dto.CloseTime, @"hh\:mm", null, out var parsed);
            closeTime = parsed;
        }

        var command = new CreateVenueClosureCommand
        {
            VenueId = venueId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsFullDay = dto.IsFullDay,
            OpenTime = openTime,
            CloseTime = closeTime,
            Reason = dto.Reason
        };

        var ids = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetClosures), new { venueId }, ids);
    }

    /// <summary>
    /// Kapalı gün günceller.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="closureId">Kapalılık ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kapalılık bulunamadı</response>
    [HttpPut("{closureId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClosure(
        Guid venueId,
        Guid closureId,
        [FromBody] UpdateVenueClosureDto dto,
        CancellationToken cancellationToken = default)
    {
        // Time string'i TimeSpan'e çevir
        TimeSpan? openTime = null;
        TimeSpan? closeTime = null;

        if (!dto.IsFullDay && !string.IsNullOrEmpty(dto.OpenTime))
        {
            TimeSpan.TryParseExact(dto.OpenTime, @"hh\:mm", null, out var parsed);
            openTime = parsed;
        }

        if (!dto.IsFullDay && !string.IsNullOrEmpty(dto.CloseTime))
        {
            TimeSpan.TryParseExact(dto.CloseTime, @"hh\:mm", null, out var parsed);
            closeTime = parsed;
        }

        var command = new UpdateVenueClosureCommand
        {
            VenueId = venueId,
            ClosureId = closureId,
            Date = dto.Date,
            IsFullDay = dto.IsFullDay,
            OpenTime = openTime,
            CloseTime = closeTime,
            Reason = dto.Reason
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kapalı gün siler.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="closureId">Kapalılık ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Kapalılık başarıyla silindi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kapalılık bulunamadı</response>
    [HttpDelete("{closureId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClosure(
        Guid venueId,
        Guid closureId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteVenueClosureCommand
        {
            VenueId = venueId,
            ClosureId = closureId
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Toplu kapalı gün ekler (maksimum 50 adet).
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Kapalılık listesi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kapalılık ID'leri</returns>
    /// <response code="201">Kapalılıklar başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya limit aşıldı</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BulkCreateClosures(
        Guid venueId,
        [FromBody] BulkCreateVenueClosureDto dto,
        CancellationToken cancellationToken = default)
    {
        var items = dto.Closures.Select(c =>
        {
            TimeSpan? openTime = null;
            TimeSpan? closeTime = null;

            if (!c.IsFullDay && !string.IsNullOrEmpty(c.OpenTime))
            {
                TimeSpan.TryParseExact(c.OpenTime, @"hh\:mm", null, out var parsed);
                openTime = parsed;
            }

            if (!c.IsFullDay && !string.IsNullOrEmpty(c.CloseTime))
            {
                TimeSpan.TryParseExact(c.CloseTime, @"hh\:mm", null, out var parsed);
                closeTime = parsed;
            }

            return new CreateVenueClosureItem
            {
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsFullDay = c.IsFullDay,
                OpenTime = openTime,
                CloseTime = closeTime,
                Reason = c.Reason
            };
        }).ToList();

        var command = new BulkCreateVenueClosureCommand
        {
            VenueId = venueId,
            Closures = items
        };

        var ids = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetClosures), new { venueId }, ids);
    }
}
