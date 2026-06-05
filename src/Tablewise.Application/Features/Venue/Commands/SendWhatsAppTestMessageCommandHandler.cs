using MediatR;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// SendWhatsAppTestMessageCommand handler'ı.
/// IMessagingChannel üzerinden test mesajı gönderir.
/// </summary>
public sealed class SendWhatsAppTestMessageCommandHandler
    : IRequestHandler<SendWhatsAppTestMessageCommand, Unit>
{
    private readonly IMessagingChannel _messagingChannel;
    private readonly ILogger<SendWhatsAppTestMessageCommandHandler> _logger;

    private const string TestMessageText =
        "🧪 Tablewise test mesajı. WhatsApp bildirimleri başarıyla yapılandırıldı!";

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public SendWhatsAppTestMessageCommandHandler(
        IMessagingChannel messagingChannel,
        ILogger<SendWhatsAppTestMessageCommandHandler> logger)
    {
        _messagingChannel = messagingChannel;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(
        SendWhatsAppTestMessageCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "WhatsApp test mesajı gönderiliyor. VenueId: {VenueId}, To: {Phone}",
            request.VenueId, MaskPhone(request.ToPhone));

        await _messagingChannel
            .SendTextAsync(request.ToPhone, TestMessageText, cancellationToken)
            .ConfigureAwait(false);

        return Unit.Value;
    }

    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7) return "***";
        return $"{phone[..4]}***{phone[^4..]}";
    }
}
