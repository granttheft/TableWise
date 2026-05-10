using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Polly;
using Polly.Retry;
using Tablewise.Application.Settings;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Email.Models;

namespace Tablewise.Infrastructure.Email.Services;

/// <summary>
/// SendGrid ile email gönderir. Retry policy, NotificationLog kayıt.
/// </summary>
public sealed class SendGridEmailService
{
    private readonly SendGridClient _client;
    private readonly EmailTemplateRenderer _renderer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly SendGridSettings _settings;
    private readonly ResiliencePipeline _retryPipeline;

    public SendGridEmailService(
        IOptions<SendGridSettings> settings,
        EmailTemplateRenderer renderer,
        IUnitOfWork unitOfWork,
        ILogger<SendGridEmailService> logger)
    {
        _settings = settings.Value;
        _client = new SendGridClient(_settings.ApiKey);
        _renderer = renderer;
        _unitOfWork = unitOfWork;
        _logger = logger;

        // Polly retry policy: 3 deneme, exponential backoff
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "SendGrid retry #{Attempt} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Ham email gönderir.
    /// </summary>
    public async Task<bool> SendAsync(EmailRequest request, CancellationToken ct = default)
    {
        try
        {
            await _retryPipeline.ExecuteAsync(async token =>
            {
                var msg = MailHelper.CreateSingleEmail(
                    new EmailAddress(_settings.FromEmail, _settings.FromName),
                    new EmailAddress(request.To),
                    request.Subject,
                    request.PlainTextBody ?? string.Empty,
                    request.HtmlBody);

                msg.SetReplyTo(new EmailAddress(_settings.ReplyTo));

                var response = await _client.SendEmailAsync(msg, token).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Body.ReadAsStringAsync(token).ConfigureAwait(false);
                    throw new Exception($"SendGrid error: {response.StatusCode}, {body}");
                }
            }, ct).ConfigureAwait(false);

            await SaveNotificationLogAsync(request, "Sent", null, ct).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendGrid email gönderimi başarısız. To={To}", request.To);
            await SaveNotificationLogAsync(request, "Failed", ex.Message, ct).ConfigureAwait(false);
            return false;
        }
    }

    /// <summary>
    /// Şablon kullanarak email gönderir.
    /// </summary>
    public async Task<bool> SendTemplatedAsync(
        string to,
        EmailTemplate template,
        Dictionary<string, object> data,
        Guid? tenantId,
        Guid? reservationId,
        NotificationType notificationType,
        CancellationToken ct = default)
    {
        var (html, plainText) = _renderer.Render(template, data);
        var subject = GetSubject(template);

        var request = new EmailRequest
        {
            TenantId = tenantId,
            ReservationId = reservationId,
            To = to,
            Subject = subject,
            HtmlBody = html,
            PlainTextBody = plainText,
            TemplateName = template.ToString(),
            NotificationType = notificationType.ToString()
        };

        return await SendAsync(request, ct).ConfigureAwait(false);
    }

    private static string GetSubject(EmailTemplate template) => template switch
    {
        EmailTemplate.Welcome => "Tablewise'a Hoş Geldiniz! 🎉",
        EmailTemplate.EmailVerification => "Email Adresinizi Doğrulayın",
        EmailTemplate.PasswordReset => "Şifre Sıfırlama Talebi",
        EmailTemplate.ReservationConfirm => "Rezervasyonunuz Onaylandı ✓",
        EmailTemplate.ReservationModified => "Rezervasyonunuz Güncellendi",
        EmailTemplate.ReservationCancelled => "Rezervasyonunuz İptal Edildi",
        EmailTemplate.ReservationReminder => "Yarın Rezervasyonunuz Var 🍽",
        EmailTemplate.NewReservationOwner => "Yeni Rezervasyon Bildirimi",
        EmailTemplate.NoShowNotification => "Rezervasyona Gelinmedi",
        EmailTemplate.StaffInvitation => "Tablewise'a Davet Edildiniz",
        EmailTemplate.TrialExpiryReminder => "Deneme Süreniz Sona Eriyor",
        EmailTemplate.PlanUpgraded => "Planınız Yükseltildi 🎉",
        EmailTemplate.PlanPaymentFailed => "Ödeme Başarısız",
        EmailTemplate.DepositPaid => "Kapora Ödemesi Alındı",
        EmailTemplate.DepositRefunded => "Kapora İade Edildi",
        _ => "Tablewise Bildirimi"
    };

    private async Task SaveNotificationLogAsync(
        EmailRequest request,
        string status,
        string? errorMessage,
        CancellationToken ct)
    {
        if (request.TenantId == null) return;

        var log = new NotificationLog
        {
            TenantId = request.TenantId.Value,
            ReservationId = request.ReservationId,
            Channel = NotificationChannel.Email,
            Type = Enum.TryParse<NotificationType>(request.NotificationType, out var type) ? type : NotificationType.Confirm,
            Recipient = MaskEmail(request.To),
            Status = status,
            ErrorMessage = errorMessage,
            SentAt = status == "Sent" ? DateTime.UtcNow : null
        };

        await _unitOfWork.NotificationLogs.AddAsync(log, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return email;
        var name = parts[0];
        var masked = name.Length > 3 ? $"{name[..3]}***" : "***";
        return $"{masked}@{parts[1]}";
    }
}
