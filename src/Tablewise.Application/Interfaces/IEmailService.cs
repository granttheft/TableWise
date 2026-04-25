namespace Tablewise.Application.Interfaces;

/// <summary>
/// Email gönderim servisi interface.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Hoşgeldin emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="userName">Kullanıcı adı</param>
    /// <param name="verificationLink">Email doğrulama linki</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Şifre sıfırlama emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="userName">Kullanıcı adı</param>
    /// <param name="resetLink">Şifre sıfırlama linki</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Email doğrulama başarılı emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="userName">Kullanıcı adı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendEmailVerifiedNotificationAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default);
}
