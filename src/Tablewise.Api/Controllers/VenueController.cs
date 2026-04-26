using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Venue;
using Tablewise.Application.Features.Venue.Commands;
using Tablewise.Application.Features.Venue.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Venue (mekan) yönetimi controller'ı.
/// Sadece Owner rolü erişebilir.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class VenueController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VenueController> _logger;

    /// <summary>
    /// VenueController constructor.
    /// </summary>
    public VenueController(
        IMediator mediator,
        ILogger<VenueController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Venue listesini getirir.
    /// </summary>
    /// <param name="activeOnly">Sadece aktif venue'ler mi?</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Venue listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<VenueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVenues(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVenuesQuery { ActiveOnly = activeOnly };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// ID'ye göre venue detayını getirir.
    /// </summary>
    /// <param name="id">Venue ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Venue detayı</returns>
    /// <response code="200">Venue bulundu</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VenueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVenueById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVenueByIdQuery { VenueId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni venue oluşturur.
    /// Plan limitlerini kontrol eder.
    /// </summary>
    /// <param name="dto">Venue bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan venue ID'si</returns>
    /// <response code="201">Venue başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya plan limiti doldu</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateVenue(
        [FromBody] CreateVenueDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateVenueCommand
        {
            Name = dto.Name,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            Description = dto.Description,
            TimeZone = dto.TimeZone,
            SlotDurationMinutes = dto.SlotDurationMinutes,
            DepositEnabled = dto.DepositEnabled,
            DepositAmount = dto.DepositAmount,
            DepositPerPerson = dto.DepositPerPerson,
            DepositRefundPolicy = dto.DepositRefundPolicy,
            DepositRefundHours = dto.DepositRefundHours,
            DepositPartialPercent = dto.DepositPartialPercent,
            WorkingHours = dto.WorkingHours
        };

        var venueId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetVenueById), new { id = venueId }, venueId);
    }

    /// <summary>
    /// Venue bilgilerini günceller.
    /// </summary>
    /// <param name="id">Venue ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVenue(
        Guid id,
        [FromBody] UpdateVenueDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateVenueCommand
        {
            VenueId = id,
            Name = dto.Name,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            Description = dto.Description,
            TimeZone = dto.TimeZone,
            SlotDurationMinutes = dto.SlotDurationMinutes,
            DepositEnabled = dto.DepositEnabled,
            DepositAmount = dto.DepositAmount,
            DepositPerPerson = dto.DepositPerPerson,
            DepositRefundPolicy = dto.DepositRefundPolicy,
            DepositRefundHours = dto.DepositRefundHours,
            DepositPartialPercent = dto.DepositPartialPercent,
            WorkingHours = dto.WorkingHours
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Venue siler (soft delete).
    /// Aktif rezervasyonu olan venue silinemez.
    /// </summary>
    /// <param name="id">Venue ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Venue başarıyla silindi</response>
    /// <response code="400">Aktif rezervasyon var, silinemez</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVenue(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteVenueCommand { VenueId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Venue çalışma saatlerini günceller.
    /// </summary>
    /// <param name="id">Venue ID</param>
    /// <param name="dto">Çalışma saatleri (JSON formatında)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz JSON formatı</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpPut("{id:guid}/working-hours")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkingHours(
        Guid id,
        [FromBody] UpdateWorkingHoursDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateWorkingHoursCommand
        {
            VenueId = id,
            WorkingHours = dto.WorkingHours
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
