using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Table;
using Tablewise.Application.Features.Table.Commands;
using Tablewise.Application.Features.Table.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Masa yönetimi controller'ı.
/// Sadece Owner rolü erişebilir.
/// </summary>
[ApiController]
[Route("api/v1/venues/{venueId:guid}/tables")]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class TableController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TableController> _logger;

    /// <summary>
    /// TableController constructor.
    /// </summary>
    public TableController(
        IMediator mediator,
        ILogger<TableController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Venue'nin masalarını getirir (sıralı).
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="activeOnly">Sadece aktif masalar mı?</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Masa listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTables(
        Guid venueId,
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTablesQuery 
        { 
            VenueId = venueId,
            ActiveOnly = activeOnly
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni masa ekler.
    /// Plan limitlerini kontrol eder (Starter: 3 masa, Pro+: sınırsız).
    /// SortOrder otomatik belirlenir.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Masa bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan masa ID'si</returns>
    /// <response code="201">Masa başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya plan limiti doldu</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTable(
        Guid venueId,
        [FromBody] CreateTableDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTableCommand
        {
            VenueId = venueId,
            Name = dto.Name,
            Capacity = dto.Capacity,
            Location = dto.Location,
            Description = dto.Description
        };

        var tableId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetTables), new { venueId }, tableId);
    }

    /// <summary>
    /// Masa günceller.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="tableId">Masa ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Masa bulunamadı</response>
    [HttpPut("{tableId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTable(
        Guid venueId,
        Guid tableId,
        [FromBody] UpdateTableDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateTableCommand
        {
            VenueId = venueId,
            TableId = tableId,
            Name = dto.Name,
            Capacity = dto.Capacity,
            Location = dto.Location,
            Description = dto.Description
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Masa siler (soft delete).
    /// Aktif rezervasyonu olan masa silinemez.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="tableId">Masa ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Masa başarıyla silindi</response>
    /// <response code="400">Aktif rezervasyon var, silinemez</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Masa bulunamadı</response>
    [HttpDelete("{tableId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTable(
        Guid venueId,
        Guid tableId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTableCommand
        {
            VenueId = venueId,
            TableId = tableId
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Masa sıralamasını günceller.
    /// Maksimum 100 masa tek seferde sıralanabilir.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Sıralama bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Sıralama başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek veya limit aşıldı</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Masalar bulunamadı</response>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderTables(
        Guid venueId,
        [FromBody] ReorderTablesDto dto,
        CancellationToken cancellationToken = default)
    {
        var orders = dto.Items.Select(item => new TableOrder
        {
            Id = item.Id,
            SortOrder = item.SortOrder
        }).ToList();

        var command = new ReorderTablesCommand
        {
            VenueId = venueId,
            Orders = orders
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Masa aktiflik durumunu değiştirir (toggle).
    /// IsActive: true → false, false → true.
    /// Aktif rezervasyonu olan masa deaktive edilemez.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="tableId">Masa ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Durum başarıyla değiştirildi</response>
    /// <response code="400">Aktif rezervasyon var, deaktive edilemez</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Masa bulunamadı</response>
    [HttpPut("{tableId:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleTableActive(
        Guid venueId,
        Guid tableId,
        CancellationToken cancellationToken = default)
    {
        var command = new ToggleTableActiveCommand
        {
            VenueId = venueId,
            TableId = tableId
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
