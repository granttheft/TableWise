using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Tablewise.Application.Settings;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Tablewise.Infrastructure.Messaging;

/// <summary>
/// Twilio WhatsApp Business API kanalı.
/// AccountSid boşsa no-op olarak çalışır (yerel geliştirme).
/// </summary>
public sealed class TwilioWhatsAppChannel : IMessagingChannel
{
    private readonly WhatsAppSettings _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TwilioWhatsAppChannel> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly bool _isConfigured;

    private static bool _missingKeyWarningLogged;

    // Şablon metinleri (Türkçe)
    private static readonly Dictionary<WhatsAppMessageTemplate, string> Templates = new()
    {
        [WhatsAppMessageTemplate.ReservationReceived] =
            "Merhaba {ad}, {mekan} rezervasyon talebiniz alındı.\n📅 {tarih} · 🕐 {saat} · 👥 {kisi} kişi\nÖdemeniz onaylandıktan sonra rezervasyonunuz kesinleşecektir.",

        [WhatsAppMessageTemplate.ReservationConfirmed] =
            "✅ {mekan} rezervasyonunuz onaylandı!\n📅 {tarih} · 🕐 {saat} · 👥 {kisi} kişi\nSizi bekliyoruz.",

        [WhatsAppMessageTemplate.Reminder] =
            "🔔 Hatırlatma: Yarın {mekan} rezervasyonunuz var.\n🕐 {saat} · 👥 {kisi} kişi",

        [WhatsAppMessageTemplate.Cancellation] =
            "{mekan} rezervasyonunuz iptal edildi.\n{iadeBilgisi}\nYeni rezervasyon: {bookingLink}",
    };

    /// <inheritdoc />
    public MessagingChannelType ChannelType => MessagingChannelType.WhatsApp;

    /// <summary>
    /// TwilioWhatsAppChannel constructor.
    /// </summary>
    public TwilioWhatsAppChannel(
        IOptions<WhatsAppSettings> settings,
        IUnitOfWork unitOfWork,
        ILogger<TwilioWhatsAppChannel> logger)
    {
        _settings = settings.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;

        _isConfigured = !string.IsNullOrWhiteSpace(_settings.AccountSid) &&
                        !string.IsNullOrWhiteSpace(_settings.AuthToken);

        if (_isConfigured)
        {
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        }
        else if (!_missingKeyWarningLogged)
        {
            _missingKeyWarningLogged = true;
            _logger.LogWarning(
                "WhatsApp:AccountSid veya AuthToken boş; WhatsApp gönderimi devre dışı. " +
                "Production öncesi appsettings.Local.json'da yapılandırın.");
        }

        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "WhatsApp gönderimi yeniden deneniyor. Deneme: {Attempt}, Bekleme: {Delay}",
                        args.AttemptNumber + 1, args.RetryDelay);
                    return ValueTask.CompletedTask;
                },
            })
            .Build();
    }

    /// <inheritdoc />
    public async Task<string?> SendTemplatedAsync(
        string toPhone,
        WhatsAppMessageTemplate template,
        Dictionary<string, string> data,
        Guid? reservationId = null,
        CancellationToken ct = default)
    {
        if (!Templates.TryGetValue(template, out var templateText))
        {
            _logger.LogError("Bilinmeyen WhatsApp şablonu: {Template}", template);
            return null;
        }

        // Placeholder'ları doldur
        var body = data.Aggregate(templateText, (current, pair) =>
            current.Replace($"{{{pair.Key}}}", pair.Value));

        return await SendTextAsync(toPhone, body, template, reservationId, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<string?> SendTextAsync(string toPhone, string body, CancellationToken ct = default)
        => SendTextAsync(toPhone, body, null, null, ct);

    /// <inheritdoc />
    public async Task<WhatsAppMessageStatus?> GetDeliveryStatusAsync(
        string providerMessageId,
        CancellationToken ct = default)
    {
        if (!_isConfigured) return null;

        try
        {
            var message = await MessageResource.FetchAsync(providerMessageId).ConfigureAwait(false);
            return MapTwilioStatus(message.Status?.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio durum sorgusu başarısız. MessageSid: {MessageSid}", providerMessageId);
            return null;
        }
    }

    // ---- Private ----

    private async Task<string?> SendTextAsync(
        string toPhone,
        string body,
        WhatsAppMessageTemplate? template,
        Guid? reservationId,
        CancellationToken ct)
    {
        var maskedPhone = MaskPhone(toPhone);
        var whatsAppRecord = template.HasValue
            ? new WhatsAppMessage
            {
                ReservationId = reservationId,
                ToPhone = maskedPhone,
                Template = template.Value,
                Status = WhatsAppMessageStatus.Queued,
            }
            : null;

        if (whatsAppRecord != null)
        {
            await _unitOfWork.WhatsAppMessages.AddAsync(whatsAppRecord, ct).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        if (!_isConfigured)
        {
            _logger.LogInformation(
                "[WhatsApp no-op] To: {Phone}, Template: {Template}, Body: {Body}",
                maskedPhone, template, body);
            return null;
        }

        string? messageSid = null;

        try
        {
            messageSid = await _retryPipeline.ExecuteAsync(async _ =>
            {
                var toWhatsApp = FormatWhatsAppNumber(toPhone);
                var msg = await MessageResource.CreateAsync(
                    to: new PhoneNumber(toWhatsApp),
                    from: new PhoneNumber(_settings.FromNumber),
                    body: body).ConfigureAwait(false);

                return msg.Sid;
            }, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "WhatsApp gönderildi. To: {Phone}, Template: {Template}, Sid: {Sid}",
                maskedPhone, template, messageSid);

            if (whatsAppRecord != null)
            {
                whatsAppRecord.ProviderMessageId = messageSid;
                whatsAppRecord.Status = WhatsAppMessageStatus.Sent;
                whatsAppRecord.SentAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "WhatsApp gönderilemedi. To: {Phone}, Template: {Template}",
                maskedPhone, template);

            if (whatsAppRecord != null)
            {
                whatsAppRecord.Status = WhatsAppMessageStatus.Failed;
                whatsAppRecord.ErrorMessage = ex.Message.Length > 500
                    ? ex.Message[..500]
                    : ex.Message;
                await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }

        return messageSid;
    }

    private static string FormatWhatsAppNumber(string phone)
    {
        // E.164 formatlama: +90... → whatsapp:+90...
        var normalized = phone.Trim();
        if (!normalized.StartsWith('+'))
            normalized = "+" + normalized;

        return normalized.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : "whatsapp:" + normalized;
    }

    private static string MaskPhone(string phone)
    {
        // +905321234567 → +905***4567
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7)
            return "***";

        var start = phone[..4];
        var end = phone[^4..];
        return $"{start}***{end}";
    }

    private static WhatsAppMessageStatus MapTwilioStatus(string? twilioStatus) =>
        twilioStatus?.ToLowerInvariant() switch
        {
            "queued" or "accepted" or "sending" => WhatsAppMessageStatus.Queued,
            "sent" => WhatsAppMessageStatus.Sent,
            "delivered" => WhatsAppMessageStatus.Delivered,
            "read" => WhatsAppMessageStatus.Read,
            "failed" or "undelivered" => WhatsAppMessageStatus.Failed,
            _ => WhatsAppMessageStatus.Queued,
        };
}
