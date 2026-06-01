using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tablewise.Application.Features.Booking.Commands;
using Tablewise.Application.Settings;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Twilio.Security;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Twilio WhatsApp webhook endpoint'i.
/// Teslimat durumu güncellemeleri ve gelen mesaj işleme.
/// </summary>
[ApiController]
[Route("api/webhooks/whatsapp")]
public sealed class WhatsAppWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<WhatsAppWebhookController> _logger;

    /// <summary>
    /// WhatsAppWebhookController constructor.
    /// </summary>
    public WhatsAppWebhookController(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        IOptions<WhatsAppSettings> settings,
        ILogger<WhatsAppWebhookController> logger)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Twilio webhook: gelen mesaj + teslimat durumu güncelleme.
    /// X-Twilio-Signature ile imza doğrulaması yapılır — imzasız istekler reddedilir.
    /// </summary>
    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ReceiveWebhook(
        [FromForm] IFormCollection form,
        CancellationToken ct)
    {
        // Twilio imza doğrulama
        if (!ValidateTwilioSignature(form))
        {
            _logger.LogWarning("Geçersiz Twilio imzası. IP: {IP}", HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }

        var messageSid = form["MessageSid"].ToString();
        var messageStatus = form["MessageStatus"].ToString();
        var messageBody = form["Body"].ToString();
        var fromPhone = form["From"].ToString(); // whatsapp:+90...

        // 1. Teslimat durumu güncelleme
        if (!string.IsNullOrEmpty(messageSid) && !string.IsNullOrEmpty(messageStatus))
        {
            await UpdateDeliveryStatusAsync(messageSid, messageStatus, ct).ConfigureAwait(false);
        }

        // 2. Gelen müşteri mesajı — "İPTAL" keyword
        if (!string.IsNullOrEmpty(messageBody) && !string.IsNullOrEmpty(fromPhone))
        {
            var normalizedBody = messageBody.Trim().ToUpperInvariant();
            if (normalizedBody is "İPTAL" or "IPTAL" or "CANCEL")
            {
                await HandleCancellationRequestAsync(fromPhone, ct).ConfigureAwait(false);
            }
        }

        // Twilio boş 200 OK bekler
        return Ok();
    }

    /// <summary>
    /// Twilio webhook doğrulama (verify token challenge — Meta uyumluluğu için).
    /// </summary>
    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? token,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (mode == "subscribe" &&
            !string.IsNullOrEmpty(token) &&
            token == _settings.WebhookVerifyToken &&
            !string.IsNullOrEmpty(challenge))
        {
            return Ok(challenge);
        }

        return Unauthorized();
    }

    // ---- Private ----

    private bool ValidateTwilioSignature(IFormCollection form)
    {
        if (string.IsNullOrWhiteSpace(_settings.AuthToken))
        {
            // Geliştirme modunda (AuthToken boş) imza doğrulamayı atla
            _logger.LogDebug("WhatsApp AuthToken boş; Twilio imza doğrulaması atlandı (dev modu).");
            return true;
        }

        var signature = Request.Headers["X-Twilio-Signature"].ToString();
        if (string.IsNullOrEmpty(signature)) return false;

        var requestUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        var parameters = form.ToDictionary(f => f.Key, f => f.Value.ToString());

        var validator = new RequestValidator(_settings.AuthToken);
        return validator.Validate(requestUrl, parameters, signature);
    }

    private async Task UpdateDeliveryStatusAsync(string messageSid, string twilioStatus, CancellationToken ct)
    {
        try
        {
            var status = MapStatus(twilioStatus);
            var message = await _unitOfWork.WhatsAppMessages
                .Query()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.ProviderMessageId == messageSid, ct)
                .ConfigureAwait(false);

            if (message == null) return;

            message.Status = status;
            if (status == WhatsAppMessageStatus.Delivered && message.DeliveredAt == null)
                message.DeliveredAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

            _logger.LogDebug(
                "WhatsApp durum güncellendi. Sid: {Sid}, Durum: {Status}", messageSid, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp durum güncellemesi başarısız. Sid: {Sid}", messageSid);
        }
    }

    private async Task HandleCancellationRequestAsync(string fromPhone, CancellationToken ct)
    {
        // Telefon numarasından son aktif rezervasyonu bul — basit keyword matching
        // "whatsapp:+905..." → "+905..."
        var phone = fromPhone.Replace("whatsapp:", "", StringComparison.OrdinalIgnoreCase).Trim();

        try
        {
            var reservation = await _unitOfWork.Reservations
                .Query()
                .IgnoreQueryFilters()
                .Where(r =>
                    r.GuestPhone == phone &&
                    !r.IsDeleted &&
                    (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Pending) &&
                    r.ReservedFor > DateTime.UtcNow)
                .OrderByDescending(r => r.ReservedFor)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (reservation == null)
            {
                _logger.LogInformation(
                    "WhatsApp iptal isteği: aktif rezervasyon bulunamadı. Phone: {Phone}", phone);
                return;
            }

            await _mediator.Send(new CancelPublicReservationCommand
            {
                ConfirmCode = reservation.ConfirmCode,
                Reason = "WhatsApp üzerinden müşteri talebiyle iptal edildi",
            }, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "WhatsApp iptal isteği işlendi. ConfirmCode: {Code}, Phone: {Phone}",
                reservation.ConfirmCode, phone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp iptal isteği işlenemedi. Phone: {Phone}", phone);
        }
    }

    private static WhatsAppMessageStatus MapStatus(string twilioStatus) =>
        twilioStatus.ToLowerInvariant() switch
        {
            "queued" or "accepted" or "sending" => WhatsAppMessageStatus.Queued,
            "sent" => WhatsAppMessageStatus.Sent,
            "delivered" => WhatsAppMessageStatus.Delivered,
            "read" => WhatsAppMessageStatus.Read,
            "failed" or "undelivered" => WhatsAppMessageStatus.Failed,
            _ => WhatsAppMessageStatus.Queued,
        };
}
