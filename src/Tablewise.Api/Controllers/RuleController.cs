using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Features.Rule.Commands;
using Tablewise.Application.Features.Rule.Queries;
using Tablewise.Application.Interfaces;
using Tablewise.Application.Services;
using Tablewise.Domain.Enums;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Kural yönetimi controller'ı.
/// Sadece Owner rolü erişebilir.
/// </summary>
[ApiController]
[Route("api/v1/rules")]
[Authorize]
[RequireOwner]
[Produces("application/json")]
public sealed class RuleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RuleController> _logger;

    /// <summary>
    /// RuleController constructor.
    /// </summary>
    public RuleController(
        IMediator mediator,
        ILogger<RuleController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Kural listesini getirir.
    /// </summary>
    /// <param name="venueId">Mekan ID filtresi (opsiyonel)</param>
    /// <param name="isActive">Aktif durum filtresi (opsiyonel)</param>
    /// <param name="triggerType">Tetikleyici tip filtresi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kural listesi</returns>
    /// <response code="200">Liste başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRules(
        [FromQuery] Guid? venueId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] RuleTrigger? triggerType = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRulesQuery
        {
            VenueId = venueId,
            IsActive = isActive,
            TriggerType = triggerType
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// ID'ye göre kural detayını getirir.
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kural detayı</returns>
    /// <response code="200">Kural bulundu</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kural bulunamadı</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRuleById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRuleByIdQuery { RuleId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni kural oluşturur.
    /// Plan limitlerini kontrol eder.
    /// </summary>
    /// <param name="dto">Kural bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kural ID'si</returns>
    /// <response code="201">Kural başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek veya plan limiti doldu</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="422">JSON validation hatası</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateRule(
        [FromBody] CreateRuleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateRuleCommand { Dto = dto };
        var ruleId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRuleById), new { id = ruleId }, ruleId);
    }

    /// <summary>
    /// Kuralı günceller.
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <param name="dto">Güncellenmiş kural bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>NoContent</returns>
    /// <response code="204">Kural başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kural bulunamadı</response>
    /// <response code="422">JSON validation hatası</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateRule(
        Guid id,
        [FromBody] UpdateRuleDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateRuleCommand { RuleId = id, Dto = dto };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kuralı siler (soft delete).
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>NoContent</returns>
    /// <response code="204">Kural başarıyla silindi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kural bulunamadı</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRule(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteRuleCommand { RuleId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kuralın aktif/pasif durumunu değiştirir.
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>NoContent</returns>
    /// <response code="204">Durum başarıyla değiştirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kural bulunamadı</response>
    [HttpPut("{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleRule(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ToggleRuleCommand { RuleId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kuralların öncelik sıralamasını günceller.
    /// </summary>
    /// <param name="dto">Sıralama bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>NoContent</returns>
    /// <response code="204">Sıralama başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReorderRules(
        [FromBody] ReorderRulesDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new ReorderRulesCommand { Dto = dto };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kuralı test eder (simüle eder - Faz 3'te gerçek motor).
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <param name="dto">Test parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Test sonucu</returns>
    /// <response code="200">Test tamamlandı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Kural bulunamadı</response>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(typeof(RuleEvaluationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestRule(
        Guid id,
        [FromBody] TestRuleRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new TestRuleCommand { RuleId = id, Dto = dto };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Kural şablonlarını getirir.
    /// </summary>
    /// <returns>Şablon listesi</returns>
    /// <response code="200">Şablonlar başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(List<RuleTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult GetTemplates()
    {
        var templates = RuleTemplatesProvider.GetAll();
        return Ok(templates);
    }

    /// <summary>
    /// Kural istatistiklerini getirir (TimesTriggered).
    /// </summary>
    /// <param name="venueId">Mekan ID filtresi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İstatistik listesi</returns>
    /// <response code="200">İstatistikler başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(List<RuleStatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats(
        [FromQuery] Guid? venueId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRuleStatsQuery { VenueId = venueId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
