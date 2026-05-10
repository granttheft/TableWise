using System.Globalization;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Email.Models;

namespace Tablewise.Infrastructure.Email.Services;

/// <summary>
/// IEmailService implementation. Tüm email çağrılarını EmailQueueService'e yönlendirir.
/// </summary>
public sealed class QueuedEmailService : IEmailService
{
    private readonly EmailQueueService _queueService;
    private readonly ITenantContext _tenantContext;

    public QueuedEmailService(EmailQueueService queueService, ITenantContext tenantContext)
    {
        _queueService = queueService;
        _tenantContext = tenantContext;
    }

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "tenantName", userName },
            { "verificationUrl", verificationLink }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.Welcome, NotificationType.Welcome, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "userName", userName },
            { "resetUrl", resetLink }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.PasswordReset, NotificationType.PasswordReset, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendEmailVerifiedNotificationAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "userName", userName },
            { "verificationUrl", "https://app.tablewise.com.tr" }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.EmailVerification, NotificationType.EmailVerification, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendStaffInvitationEmailAsync(
        string toEmail,
        string tenantName,
        string inviterName,
        string role,
        string inviteLink,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "tenantName", tenantName },
            { "inviterName", inviterName },
            { "role", role },
            { "inviteUrl", inviteLink }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.StaffInvitation, NotificationType.StaffInvitation, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendReservationConfirmationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "guestName", guestName },
            { "venueName", venueName },
            { "reservedFor", reservedFor.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) },
            { "confirmCode", confirmCode },
            { "partySize", "4" },
            { "manageUrl", "https://app.tablewise.com.tr/manage" }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.ReservationConfirm, NotificationType.Confirm, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendReservationCancellationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "guestName", guestName },
            { "venueName", venueName },
            { "reservedFor", reservedFor.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) },
            { "confirmCode", confirmCode },
            { "bookingUrl", "https://app.tablewise.com.tr/book" }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.ReservationCancelled, NotificationType.Cancel, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendReservationModificationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime oldDateTime,
        DateTime newDateTime,
        string newConfirmCode,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "guestName", guestName },
            { "venueName", venueName },
            { "oldDateTime", oldDateTime.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) },
            { "newDateTime", newDateTime.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) },
            { "confirmCode", newConfirmCode },
            { "manageUrl", "https://app.tablewise.com.tr/manage" }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.ReservationModified, NotificationType.Modified, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendReservationReminderAsync(
        string toEmail,
        string guestName,
        string venueName,
        string? venueAddress,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "guestName", guestName },
            { "venueName", venueName },
            { "venueAddress", venueAddress ?? "Adres bilgisi mevcut değil" },
            { "reservedFor", reservedFor.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) },
            { "confirmCode", confirmCode },
            { "manageUrl", "https://app.tablewise.com.tr/manage" }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.ReservationReminder, NotificationType.Reminder, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        var request = new EmailRequest
        {
            TenantId = _tenantContext.TenantId,
            To = to,
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainTextBody,
            TemplateName = "custom",
            NotificationType = NotificationType.Confirm.ToString()
        };

        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendTemplatedAsync(
        string to,
        EmailTemplate template,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        var request = CreateEmailRequest(to, template, NotificationType.Confirm, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendTrialExpiryReminderAsync(
        string toEmail,
        string tenantName,
        int daysLeft,
        string upgradeUrl,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "tenantName", tenantName },
            { "daysLeft", daysLeft.ToString() },
            { "upgradeUrl", upgradeUrl }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.TrialExpiryReminder, NotificationType.TrialExpiry, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendNewReservationToOwnerAsync(
        string toEmail,
        string ownerName,
        string guestName,
        DateTime reservedFor,
        int partySize,
        string venueName,
        string adminUrl,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "ownerName", ownerName },
            { "guestName", guestName },
            { "reservedFor", reservedFor.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) },
            { "partySize", partySize.ToString() },
            { "venueName", venueName },
            { "adminUrl", adminUrl }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.NewReservationOwner, NotificationType.NewReservationOwner, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendNoShowNotificationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "guestName", guestName },
            { "venueName", venueName },
            { "reservedFor", reservedFor.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.NoShowNotification, NotificationType.NoShow, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendPlanUpgradedAsync(
        string toEmail,
        string tenantName,
        string newPlan,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "tenantName", tenantName },
            { "newPlan", newPlan }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.PlanUpgraded, NotificationType.PlanUpgraded, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendPlanPaymentFailedAsync(
        string toEmail,
        string tenantName,
        string planName,
        DateTime dueDate,
        string paymentUrl,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "tenantName", tenantName },
            { "planName", planName },
            { "dueDate", dueDate.ToString("dd MMMM yyyy", new CultureInfo("tr-TR")) },
            { "paymentUrl", paymentUrl }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.PlanPaymentFailed, NotificationType.PlanPaymentFailed, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendDepositPaidAsync(
        string toEmail,
        string guestName,
        decimal amount,
        string venueName,
        DateTime reservedFor,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "guestName", guestName },
            { "amount", amount.ToString("F2") },
            { "venueName", venueName },
            { "reservedFor", reservedFor.ToString("dd MMMM yyyy HH:mm", new CultureInfo("tr-TR")) },
            { "manageUrl", "https://app.tablewise.com.tr/manage" }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.DepositPaid, NotificationType.DepositPaid, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendDepositRefundedAsync(
        string toEmail,
        string guestName,
        decimal amount,
        string venueName,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "guestName", guestName },
            { "amount", amount.ToString("F2") },
            { "venueName", venueName },
            { "reason", reason },
            { "bookingUrl", "https://app.tablewise.com.tr/book" }
        };

        var request = CreateEmailRequest(toEmail, EmailTemplate.DepositRefunded, NotificationType.DepositRefunded, data);
        await _queueService.EnqueueAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private EmailRequest CreateEmailRequest(
        string to,
        EmailTemplate template,
        NotificationType notificationType,
        Dictionary<string, object> data)
    {
        return new EmailRequest
        {
            TenantId = _tenantContext.TenantId,
            To = to,
            TemplateName = template.ToString(),
            NotificationType = notificationType.ToString(),
            Subject = string.Empty,
            HtmlBody = string.Empty
        };
    }
}
