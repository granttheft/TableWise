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

    /// <inheritdoc />
    public Task SendStaffInvitationEmailAsync(
        string toEmail,
        string tenantName,
        string inviterName,
        string role,
        string inviteLink,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Personel davet emaili: To={Email}, Tenant={TenantName}, Inviter={InviterName}, Role={Role}, Link={Link}",
            toEmail, tenantName, inviterName, role, inviteLink);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendReservationConfirmationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Rezervasyon onay: To={Email}, Guest={GuestName}, Venue={VenueName}, Date={Date}, Code={Code}",
            toEmail, guestName, venueName, reservedFor, confirmCode);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendReservationCancellationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Rezervasyon iptal: To={Email}, Guest={GuestName}, Venue={VenueName}, Date={Date}, Code={Code}",
            toEmail, guestName, venueName, reservedFor, confirmCode);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendReservationModificationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime oldDateTime,
        DateTime newDateTime,
        string newConfirmCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Rezervasyon değişiklik: To={Email}, Guest={GuestName}, Venue={VenueName}, OldDate={OldDate}, NewDate={NewDate}, NewCode={Code}",
            toEmail, guestName, venueName, oldDateTime, newDateTime, newConfirmCode);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendReservationReminderAsync(
        string toEmail,
        string guestName,
        string venueName,
        string? venueAddress,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL] Rezervasyon hatırlatma: To={Email}, Guest={GuestName}, Venue={VenueName}, Address={Address}, Date={Date}, Code={Code}",
            toEmail, guestName, venueName, venueAddress, reservedFor, confirmCode);

        return Task.CompletedTask;
    }
}
