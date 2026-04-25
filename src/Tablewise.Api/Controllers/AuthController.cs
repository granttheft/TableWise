using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tablewise.Application.DTOs.Auth;
using Tablewise.Application.Interfaces;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Authentication controller. Kayıt, giriş, token yenileme ve şifre işlemleri.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// AuthController constructor.
    /// </summary>
    /// <param name="authService">Auth servisi</param>
    /// <param name="logger">Logger</param>
    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Yeni tenant ve owner kullanıcı kaydı oluşturur.
    /// Starter plan trial (14 gün) ile başlatılır.
    /// </summary>
    /// <param name="dto">Kayıt bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Auth sonucu (token + kullanıcı bilgileri)</returns>
    /// <response code="201">Kayıt başarılı</response>
    /// <response code="400">Validation hatası</response>
    /// <response code="409">Email zaten kayıtlı</response>
    /// <response code="422">Doğrulama hatası</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterTenantDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authService.RegisterTenantAsync(dto, ipAddress, userAgent, cancellationToken);

        _logger.LogInformation("Yeni tenant kaydı: {TenantId}", result.Tenant.Id);

        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Kullanıcı girişi yapar.
    /// Brute-force koruması: 5 başarısız deneme → 15 dk kilit.
    /// </summary>
    /// <param name="dto">Giriş bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Auth sonucu</returns>
    /// <response code="200">Giriş başarılı</response>
    /// <response code="400">Geçersiz kimlik bilgileri</response>
    /// <response code="401">Email doğrulanmamış</response>
    /// <response code="403">Hesap askıya alınmış</response>
    /// <response code="429">Çok fazla deneme</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authService.LoginAsync(dto, ipAddress, userAgent, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Refresh token ile yeni access token alır.
    /// Token rotation uygulanır (eski token invalidate edilir).
    /// </summary>
    /// <param name="dto">Refresh token</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yeni token çifti</returns>
    /// <response code="200">Token yenileme başarılı</response>
    /// <response code="401">Geçersiz veya süresi dolmuş refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress, userAgent, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Çıkış yapar. Refresh token revoke edilir.
    /// </summary>
    /// <param name="dto">Revoke edilecek refresh token</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>No content</returns>
    /// <response code="204">Çıkış başarılı</response>
    /// <response code="401">Yetkisiz</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        var ipAddress = GetClientIpAddress();

        await _authService.LogoutAsync(dto.RefreshToken, ipAddress, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Email doğrulama işlemi yapar.
    /// </summary>
    /// <param name="dto">Doğrulama token'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Success response</returns>
    /// <response code="200">Email doğrulandı</response>
    /// <response code="400">Geçersiz veya süresi dolmuş token</response>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.VerifyEmailAsync(dto.Token, cancellationToken);

        return Ok(new { message = "Email başarıyla doğrulandı. Artık giriş yapabilirsiniz." });
    }

    /// <summary>
    /// Şifre sıfırlama emaili gönderir.
    /// Güvenlik: Email bulunamasa bile 200 döner.
    /// </summary>
    /// <param name="dto">Email adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Success response</returns>
    /// <response code="200">İstek alındı</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(dto.Email, cancellationToken);

        return Ok(new { message = "Eğer bu email adresi sistemde kayıtlıysa, şifre sıfırlama linki gönderildi." });
    }

    /// <summary>
    /// Şifre sıfırlama işlemi yapar.
    /// Tüm aktif oturumlar sonlandırılır.
    /// </summary>
    /// <param name="dto">Token ve yeni şifre</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Success response</returns>
    /// <response code="200">Şifre sıfırlandı</response>
    /// <response code="400">Geçersiz veya süresi dolmuş token</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword, cancellationToken);

        return Ok(new { message = "Şifreniz başarıyla değiştirildi. Lütfen yeni şifrenizle giriş yapın." });
    }

    #region Private Helpers

    /// <summary>
    /// İstemci IP adresini alır. Proxy arkasında çalışıyorsa X-Forwarded-For header'ına bakar.
    /// </summary>
    private string? GetClientIpAddress()
    {
        // X-Forwarded-For header (load balancer/proxy arkası)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // İlk IP adresi gerçek client IP'sidir
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        // X-Real-IP header (Nginx)
        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Doğrudan bağlantı
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// User-Agent header'ını alır.
    /// </summary>
    private string? GetUserAgent()
    {
        return Request.Headers.UserAgent.FirstOrDefault();
    }

    #endregion
}
