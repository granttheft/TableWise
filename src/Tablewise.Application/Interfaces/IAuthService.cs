using Tablewise.Application.DTOs.Auth;

namespace Tablewise.Application.Interfaces;

/// <summary>
/// Authentication ve authorization işlemleri için servis interface.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Yeni tenant ve owner kullanıcı kaydı oluşturur.
    /// Starter plan trial ile başlatır.
    /// </summary>
    /// <param name="dto">Kayıt bilgileri</param>
    /// <param name="ipAddress">İstek IP adresi (audit için)</param>
    /// <param name="userAgent">User agent (cihaz tespiti için)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Auth sonucu (token + kullanıcı bilgileri)</returns>
    Task<AuthResultDto> RegisterTenantAsync(
        RegisterTenantDto dto,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı girişi yapar. Brute-force koruması uygular.
    /// </summary>
    /// <param name="dto">Giriş bilgileri</param>
    /// <param name="ipAddress">İstek IP adresi</param>
    /// <param name="userAgent">User agent</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Auth sonucu</returns>
    Task<AuthResultDto> LoginAsync(
        LoginDto dto,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh token ile yeni access token alır.
    /// Token rotation uygular.
    /// </summary>
    /// <param name="refreshToken">Mevcut refresh token</param>
    /// <param name="ipAddress">İstek IP adresi</param>
    /// <param name="userAgent">User agent</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yeni token çifti</returns>
    Task<TokenResponseDto> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Çıkış yapar. Refresh token'ı revoke eder.
    /// </summary>
    /// <param name="refreshToken">Revoke edilecek refresh token</param>
    /// <param name="ipAddress">İstek IP adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task LogoutAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Email doğrulama işlemi yapar.
    /// </summary>
    /// <param name="token">Email doğrulama token'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Şifre sıfırlama emaili gönderir.
    /// Email bulunamasa bile başarılı döner (güvenlik).
    /// </summary>
    /// <param name="email">Email adresi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Şifre sıfırlama işlemi yapar.
    /// </summary>
    /// <param name="token">Şifre sıfırlama token'ı</param>
    /// <param name="newPassword">Yeni şifre</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Başarılı ise true</returns>
    Task<bool> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}
