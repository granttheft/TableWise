using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tablewise.Application.DTOs.Booking;
using Tablewise.Application.Features.Booking.Commands;
using Tablewise.Application.Features.Booking.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Public booking controller. JWT gerektirmez.
/// Slug bazlı mekan erişimi sağlar.
/// </summary>
[ApiController]
[Route("api/v1/book")]
[Produces("application/json")]
public sealed class BookingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingController> _logger;

    /// <summary>
    /// BookingController constructor.
    /// </summary>
    public BookingController(
        IMediator mediator,
        ILogger<BookingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Mekan yapılandırmasını getirir (booking UI için).
    /// </summary>
    /// <param name="slug">Mekan slug</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Mekan yapılandırması</returns>
    /// <response code="200">Yapılandırma başarıyla getirildi</response>
    /// <response code="404">Mekan bulunamadı</response>
    [HttpGet("{slug}/config")]
    [EnableRateLimiting("booking")]
    [ProducesResponseType(typeof(VenueConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVenueConfig(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVenueConfigQuery { Slug = slug };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Müsait slotları getirir.
    /// </summary>
    /// <param name="slug">Mekan slug</param>
    /// <param name="date">Tarih (YYYY-MM-DD)</param>
    /// <param name="partySize">Kişi sayısı</param>
    /// <param name="tableId">Belirli masa (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Müsait slotlar</returns>
    /// <response code="200">Slotlar başarıyla getirildi</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="404">Mekan bulunamadı</response>
    [HttpGet("{slug}/availability")]
    [EnableRateLimiting("booking")]
    [ProducesResponseType(typeof(AvailabilityResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailability(
        string slug,
        [FromQuery] DateTime date,
        [FromQuery] int partySize,
        [FromQuery] Guid? tableId = null,
        CancellationToken cancellationToken = default)
    {
        if (partySize < 1 || partySize > 50)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Validation Error",
                Detail = "Kişi sayısı 1-50 arasında olmalıdır."
            });
        }

        var query = new GetAvailableSlotsQuery
        {
            Slug = slug,
            Date = date,
            PartySize = partySize,
            TableId = tableId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kuralları ön izleme (kayıt oluşturmadan).
    /// </summary>
    /// <param name="slug">Mekan slug</param>
    /// <param name="dto">Değerlendirme parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kural değerlendirme sonucu</returns>
    /// <response code="200">Değerlendirme başarılı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="404">Mekan bulunamadı</response>
    [HttpPost("{slug}/evaluate")]
    [EnableRateLimiting("booking")]
    [ProducesResponseType(typeof(EvaluateRulesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateRules(
        string slug,
        [FromBody] EvaluateRulesRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new EvaluateRulesCommand
        {
            Slug = slug,
            PartySize = dto.PartySize,
            ReservedFor = dto.ReservedFor,
            TableId = dto.TableId,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rezervasyon oluşturur. Idempotency-Key header zorunlu.
    /// </summary>
    /// <param name="slug">Mekan slug</param>
    /// <param name="dto">Rezervasyon bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan rezervasyon</returns>
    /// <response code="201">Rezervasyon başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya Idempotency-Key eksik</response>
    /// <response code="404">Mekan bulunamadı</response>
    /// <response code="409">Slot müsait değil</response>
    /// <response code="422">Kural ihlali</response>
    [HttpPost("{slug}/reserve")]
    [EnableRateLimiting("reserve")]
    [ProducesResponseType(typeof(ReserveResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reserve(
        string slug,
        [FromBody] ReserveRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        // Idempotency-Key middleware tarafından kontrol edilir
        var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();

        if (!dto.PrivacyPolicyAccepted)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Validation Error",
                Detail = "Gizlilik politikası onayı zorunludur."
            });
        }

        var command = new ReserveCommand
        {
            Slug = slug,
            IdempotencyKey = idempotencyKey,
            GuestName = dto.GuestName,
            GuestEmail = dto.GuestEmail,
            GuestPhone = dto.GuestPhone,
            PartySize = dto.PartySize,
            ReservedFor = dto.ReservedFor,
            TableId = dto.TableId,
            TableCombinationId = dto.TableCombinationId,
            SpecialRequests = dto.SpecialRequests,
            CustomFieldAnswers = dto.CustomFieldAnswers
        };

        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation(
            "Rezervasyon oluşturuldu. Slug: {Slug}, ConfirmCode: {Code}",
            slug, result.ConfirmCode);

        return CreatedAtAction(nameof(GetReservationByCode), new { code = result.ConfirmCode }, result);
    }

    /// <summary>
    /// Onay kodu ile rezervasyon detayını getirir.
    /// </summary>
    /// <param name="code">Onay kodu (8 karakter)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Rezervasyon detayı</returns>
    /// <response code="200">Rezervasyon bulundu</response>
    /// <response code="404">Rezervasyon bulunamadı</response>
    [HttpGet("confirm/{code}")]
    [EnableRateLimiting("booking")]
    [ProducesResponseType(typeof(ReservationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReservationByCodeQuery { ConfirmCode = code.ToUpperInvariant() };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rezervasyonu iptal eder (müşteri tarafından).
    /// En az 24 saat öncesinden yapılmalıdır.
    /// </summary>
    /// <param name="code">Onay kodu</param>
    /// <param name="dto">İptal bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İptal sonucu</returns>
    /// <response code="200">İptal başarılı</response>
    /// <response code="400">24 saatten az süre kaldı</response>
    /// <response code="404">Rezervasyon bulunamadı</response>
    [HttpPost("confirm/{code}/cancel")]
    [EnableRateLimiting("booking")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(
        string code,
        [FromBody] CancelReservationRequestDto? dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelPublicReservationCommand
        {
            ConfirmCode = code.ToUpperInvariant(),
            Reason = dto?.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Rezervasyon müşteri tarafından iptal edildi. Code: {Code}", code);

        return Ok(new { success = result, message = "Rezervasyon başarıyla iptal edildi." });
    }

    /// <summary>
    /// Rezervasyonu değiştirir (müşteri tarafından).
    /// En az 24 saat öncesinden yapılmalıdır. Idempotency-Key zorunlu.
    /// </summary>
    /// <param name="code">Onay kodu</param>
    /// <param name="dto">Değişiklik bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yeni rezervasyon</returns>
    /// <response code="200">Değişiklik başarılı</response>
    /// <response code="400">Geçersiz istek veya 24 saatten az süre kaldı</response>
    /// <response code="404">Rezervasyon bulunamadı</response>
    /// <response code="409">Yeni slot müsait değil</response>
    [HttpPost("confirm/{code}/modify")]
    [EnableRateLimiting("reserve")]
    [ProducesResponseType(typeof(ReserveResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ModifyReservation(
        string code,
        [FromBody] ModifyReservationRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();

        var command = new ModifyReservationCommand
        {
            ConfirmCode = code.ToUpperInvariant(),
            NewDateTime = dto.NewDateTime,
            NewTableId = dto.NewTableId,
            NewPartySize = dto.NewPartySize,
            IdempotencyKey = idempotencyKey
        };

        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation(
            "Rezervasyon değiştirildi. Eski: {OldCode}, Yeni: {NewCode}",
            code, result.ConfirmCode);

        return Ok(result);
    }
}
