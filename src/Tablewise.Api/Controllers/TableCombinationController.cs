using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.TableCombination;
using Tablewise.Application.Features.TableCombination.Commands;
using Tablewise.Application.Features.TableCombination.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Masa kombinasyonu yönetimi controller'ı.
/// Sadece Owner rolü erişebilir.
/// </summary>
[ApiController]
[Route("api/v1/venues/{venueId:guid}/combinations")]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class TableCombinationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TableCombinationController> _logger;

    /// <summary>
    /// TableCombinationController constructor.
    /// </summary>
    public TableCombinationController(
        IMediator mediator,
        ILogger<TableCombinationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Venue'nin masa kombinasyonlarını getirir.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kombinasyon listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TableCombinationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCombinations(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTableCombinationsQuery { VenueId = venueId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni masa kombinasyonu oluşturur.
    /// Minimum 2 masa, maksimum 10 masa birleştirilebilir.
    /// CombinedCapacity belirtilmezse otomatik hesaplanır.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Kombinasyon bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kombinasyon ID'si</returns>
    /// <response code="201">Kombinasyon başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya kurallar ihlal edildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue veya masalar bulunamadı</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCombination(
        Guid venueId,
        [FromBody] CreateTableCombinationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTableCombinationCommand
        {
            VenueId = venueId,
            Name = dto.Name,
            TableIds = dto.TableIds,
            CombinedCapacity = dto.CombinedCapacity
        };

        var combinationId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetCombinations), new { venueId }, combinationId);
    }

    /// <summary>
    /// Masa kombinasyonu günceller.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="combinationId">Kombinasyon ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kombinasyon bulunamadı</response>
    [HttpPut("{combinationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCombination(
        Guid venueId,
        Guid combinationId,
        [FromBody] UpdateTableCombinationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateTableCombinationCommand
        {
            VenueId = venueId,
            CombinationId = combinationId,
            Name = dto.Name,
            TableIds = dto.TableIds,
            CombinedCapacity = dto.CombinedCapacity
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Masa kombinasyonu siler (soft delete).
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="combinationId">Kombinasyon ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Kombinasyon başarıyla silindi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kombinasyon bulunamadı</response>
    [HttpDelete("{combinationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCombination(
        Guid venueId,
        Guid combinationId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTableCombinationCommand
        {
            VenueId = venueId,
            CombinationId = combinationId
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
