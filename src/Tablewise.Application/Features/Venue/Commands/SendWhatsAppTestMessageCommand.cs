using MediatR;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// WhatsApp test mesajı gönderir. Twilio yapılandırılmamışsa no-op.
/// </summary>
public sealed record SendWhatsAppTestMessageCommand : IRequest<Unit>
{
    /// <summary>
    /// Test gönderiminin ilişkilendirileceği Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Test mesajının gönderileceği telefon numarası (E.164 formatı).
    /// </summary>
    public required string ToPhone { get; init; }
}
