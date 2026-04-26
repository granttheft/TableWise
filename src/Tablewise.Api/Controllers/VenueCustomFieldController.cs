using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.VenueCustomField;
using Tablewise.Application.Features.VenueCustomField.Commands;
using Tablewise.Application.Features.VenueCustomField.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Venue custom field yönetimi controller'ı.
/// Sadece Owner rolü erişebilir.
/// </summary>
[ApiController]
[Route("api/v1/venues/{venueId:guid}/custom-fields")]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class VenueCustomFieldController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VenueCustomFieldController> _logger;

    /// <summary>
    /// VenueCustomFieldController constructor.
    /// </summary>
    public VenueCustomFieldController(
        IMediator mediator,
        ILogger<VenueCustomFieldController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Custom field listesini getirir.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Custom field listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<VenueCustomFieldDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomFields(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVenueCustomFieldsQuery { VenueId = venueId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni custom field ekler.
    /// SortOrder otomatik olarak belirlenir (maks + 1).
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Custom field bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan custom field ID'si</returns>
    /// <response code="201">Custom field başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya label zaten mevcut</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Venue bulunamadı</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCustomField(
        Guid venueId,
        [FromBody] CreateVenueCustomFieldDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateVenueCustomFieldCommand
        {
            VenueId = venueId,
            Label = dto.Label,
            FieldType = dto.FieldType,
            IsRequired = dto.IsRequired,
            Options = dto.Options
        };

        var customFieldId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetCustomFields), new { venueId }, customFieldId);
    }

    /// <summary>
    /// Custom field günceller.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="customFieldId">Custom field ID</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Custom field bulunamadı</response>
    [HttpPut("{customFieldId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomField(
        Guid venueId,
        Guid customFieldId,
        [FromBody] UpdateVenueCustomFieldDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateVenueCustomFieldCommand
        {
            VenueId = venueId,
            CustomFieldId = customFieldId,
            Label = dto.Label,
            FieldType = dto.FieldType,
            IsRequired = dto.IsRequired,
            Options = dto.Options
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Custom field siler (soft delete).
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="customFieldId">Custom field ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Custom field başarıyla silindi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Custom field bulunamadı</response>
    [HttpDelete("{customFieldId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomField(
        Guid venueId,
        Guid customFieldId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteVenueCustomFieldCommand
        {
            VenueId = venueId,
            CustomFieldId = customFieldId
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Custom field sıralamasını günceller.
    /// </summary>
    /// <param name="venueId">Venue ID</param>
    /// <param name="dto">Sıralama bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Sıralama başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Custom field'lar bulunamadı</response>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderCustomFields(
        Guid venueId,
        [FromBody] ReorderCustomFieldsDto dto,
        CancellationToken cancellationToken = default)
    {
        var orders = dto.Items.Select(item => new CustomFieldOrder
        {
            Id = item.Id,
            SortOrder = item.SortOrder
        }).ToList();

        var command = new ReorderCustomFieldsCommand
        {
            VenueId = venueId,
            Orders = orders
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
