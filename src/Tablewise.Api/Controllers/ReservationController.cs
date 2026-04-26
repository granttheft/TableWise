using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tablewise.Application.DTOs.Reservation;
using Tablewise.Application.Features.Reservation.Commands;
using Tablewise.Application.Features.Reservation.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Rezervasyon yönetimi controller'ı (staff/owner için).
/// JWT authentication zorunlu.
/// </summary>
[ApiController]
[Route("api/v1/reservations")]
[Authorize]
[EnableRateLimiting("authenticated")]
[Produces("application/json")]
public sealed class ReservationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservationController> _logger;

    /// <summary>
    /// ReservationController constructor.
    /// </summary>
    public ReservationController(
        IMediator mediator,
        ILogger<ReservationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Rezervasyon listesini getirir.
    /// </summary>
    /// <param name="venueId">Mekan filtresi</param>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="status">Durum filtresi (virgülle ayrılmış)</param>
    /// <param name="search">Arama terimi</param>
    /// <param name="tableId">Masa filtresi</param>
    /// <param name="page">Sayfa numarası</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <param name="sortBy">Sıralama alanı</param>
    /// <param name="sortDirection">Sıralama yönü</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Rezervasyon listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ReservationListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReservations(
        [FromQuery] Guid? venueId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] Guid? tableId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "ReservedFor",
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetReservationsQuery
        {
            VenueId = venueId,
            FromDate = fromDate,
            ToDate = toDate,
            Status = status,
            Search = search,
            TableId = tableId,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 100),
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// ID ile rezervasyon detayını getirir.
    /// </summary>
    /// <param name="id">Rezervasyon ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Rezervasyon detayı</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReservationByIdQuery { ReservationId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Manuel rezervasyon oluşturur.
    /// </summary>
    /// <param name="dto">Rezervasyon bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan rezervasyon</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateManualReservationCommand
        {
            VenueId = dto.VenueId,
            TableId = dto.TableId,
            TableCombinationId = dto.TableCombinationId,
            GuestName = dto.GuestName,
            GuestEmail = dto.GuestEmail,
            GuestPhone = dto.GuestPhone,
            PartySize = dto.PartySize,
            ReservedFor = dto.ReservedFor,
            SpecialRequests = dto.SpecialRequests,
            InternalNotes = dto.InternalNotes,
            BypassDeposit = dto.BypassDeposit,
            BypassRules = dto.BypassRules,
            SendConfirmationEmail = dto.SendConfirmationEmail
        };

        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Manuel rezervasyon oluşturuldu. Id: {Id}", result.Id);

        return CreatedAtAction(nameof(GetReservationById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Rezervasyon durumunu günceller (Completed, NoShow).
    /// </summary>
    /// <param name="id">Rezervasyon ID</param>
    /// <param name="dto">Yeni durum</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncelleme sonucu</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateReservationStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateReservationStatusCommand
        {
            ReservationId = id,
            Status = dto.Status,
            Reason = dto.Reason
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Rezervasyonu işletme olarak iptal eder.
    /// </summary>
    /// <param name="id">Rezervasyon ID</param>
    /// <param name="dto">İptal bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İptal sonucu</returns>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(
        Guid id,
        [FromBody] CancelReservationByStaffDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelReservationByStaffCommand
        {
            ReservationId = id,
            Reason = dto.Reason,
            SendNotification = dto.SendNotification,
            RefundDeposit = dto.RefundDeposit
        };

        await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Rezervasyon işletme tarafından iptal edildi. Id: {Id}", id);

        return NoContent();
    }

    /// <summary>
    /// Rezervasyona internal not ekler.
    /// </summary>
    /// <param name="id">Rezervasyon ID</param>
    /// <param name="dto">Not içeriği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ekleme sonucu</returns>
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddInternalNote(
        Guid id,
        [FromBody] AddInternalNoteDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new AddInternalNoteCommand
        {
            ReservationId = id,
            Note = dto.Note
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Rezervasyonları CSV olarak export eder.
    /// </summary>
    /// <param name="venueId">Mekan filtresi</param>
    /// <param name="fromDate">Başlangıç tarihi (varsayılan: bu ay başı)</param>
    /// <param name="toDate">Bitiş tarihi (varsayılan: bu ay sonu)</param>
    /// <param name="status">Durum filtresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>CSV dosyası</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportReservations(
        [FromQuery] Guid? venueId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportReservationsQuery
        {
            VenueId = venueId,
            FromDate = fromDate,
            ToDate = toDate,
            Status = status
        };

        var result = await _mediator.Send(query, cancellationToken);

        return File(result.Content, result.ContentType, result.FileName);
    }
}
