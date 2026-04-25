using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Placeholder email servisi. Development'ta email'leri log'a yazar.
/// Production'da SendGrid implementation ile değiştirilecek.
/// </summary>
public sealed class PlaceholderEmailService : IEmailService
{
    private readonly ILogger<PlaceholderEmailService> _logger;

    /// <summary>
    /// PlaceholderEmailService constructor.
    /// </summary>
    public PlaceholderEmailService(ILogger<PlaceholderEmailService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Hoşgeldin emaili: To={Email}, User={UserName}, VerificationLink={Link}",
            toEmail, userName, verificationLink);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Şifre sıfırlama emaili: To={Email}, User={UserName}, ResetLink={Link}",
            toEmail, userName, resetLink);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendEmailVerifiedNotificationAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Email doğrulandı bildirimi: To={Email}, User={UserName}",
            toEmail, userName);

        return Task.CompletedTask;
    }
}
