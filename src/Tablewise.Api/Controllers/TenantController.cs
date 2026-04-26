using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Api.Authorization;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Features.Tenant.Commands;
using Tablewise.Application.Features.Tenant.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Tenant yönetimi controller'ı.
/// Tenant profil bilgileri, ayarlar, kullanım istatistikleri ve audit log işlemleri.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class TenantController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantController> _logger;

    /// <summary>
    /// TenantController constructor.
    /// </summary>
    public TenantController(
        IMediator mediator,
        ILogger<TenantController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Tenant profil bilgilerini getirir.
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tenant profil bilgileri</returns>
    /// <response code="200">Profil bilgileri başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="404">Tenant bulunamadı</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(TenantProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var query = new GetTenantProfileQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tenant bilgilerini günceller.
    /// Sadece Owner rolü erişebilir.
    /// </summary>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Güncelleme başarılı</response>
    /// <response code="400">Geçersiz istek</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Tenant bulunamadı</response>
    [HttpPut("profile")]
    [RequireOwner]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateTenantDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTenantCommand
        {
            Name = dto.Name,
            Settings = dto.Settings
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Tenant kullanım istatistiklerini getirir.
    /// Plan limitlerine göre mevcut kullanımı gösterir.
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanım istatistikleri</returns>
    /// <response code="200">İstatistikler başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    [HttpGet("usage")]
    [ProducesResponseType(typeof(TenantUsageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        var query = new GetTenantUsageQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tenant audit log'larını getirir (sayfalı).
    /// Sadece Owner rolü erişebilir.
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 50, max: 100)</param>
    /// <param name="action">Filtreleme - Action türü</param>
    /// <param name="entityType">Filtreleme - Entity tipi</param>
    /// <param name="fromDate">Filtreleme - Başlangıç tarihi (UTC)</param>
    /// <param name="toDate">Filtreleme - Bitiş tarihi (UTC)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalı audit log listesi</returns>
    /// <response code="200">Audit log listesi başarıyla getirildi</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpGet("audit-logs")]
    [RequireOwner]
    [ProducesResponseType(typeof(PagedAuditLogsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        // PageSize limiti
        if (pageSize > 100) pageSize = 100;
        if (pageSize < 1) pageSize = 1;
        if (pageNumber < 1) pageNumber = 1;

        var query = new GetAuditLogsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Action = action,
            EntityType = entityType,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logo upload için presigned URL oluşturur.
    /// Client bu URL'e doğrudan PUT request ile logo yükleyebilir.
    /// Sadece Owner rolü erişebilir.
    /// </summary>
    /// <param name="dto">Dosya bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Presigned upload URL ve metadata</returns>
    /// <response code="200">Upload URL başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz dosya tipi veya boyutu</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    [HttpPost("logo/upload-url")]
    [RequireOwner]
    [ProducesResponseType(typeof(LogoUploadUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateLogoUploadUrl(
        [FromBody] GenerateLogoUploadUrlDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateLogoUploadUrlCommand
        {
            ContentType = dto.ContentType,
            FileSizeBytes = dto.FileSizeBytes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logo upload'ını onaylar ve tenant profili ile ilişkilendirir.
    /// Upload tamamlandıktan sonra çağrılmalıdır.
    /// Sadece Owner rolü erişebilir.
    /// </summary>
    /// <param name="dto">Onaylama bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Logo başarıyla onaylandı</response>
    /// <response code="400">Geçersiz fileKey veya dosya bulunamadı</response>
    /// <response code="401">Yetkisiz</response>
    /// <response code="403">Owner yetkisi gerekli</response>
    /// <response code="404">Tenant bulunamadı</response>
    [HttpPost("logo/confirm")]
    [RequireOwner]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmLogoUpload(
        [FromBody] ConfirmLogoUploadDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new ConfirmLogoUploadCommand
        {
            FileKey = dto.FileKey
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
