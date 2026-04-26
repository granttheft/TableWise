using MediatR;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Features.Booking.Commands;

/// <summary>
/// Rezervasyon oluşturma komutu.
/// </summary>
public sealed record ReserveCommand : IRequest<ReserveResponseDto>
{
    /// <summary>
    /// Tenant slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Idempotency key.
    /// </summary>
    public string IdempotencyKey { get; init; } = string.Empty;

    /// <summary>
    /// Misafir adı.
    /// </summary>
    public string GuestName { get; init; } = string.Empty;

    /// <summary>
    /// Misafir email.
    /// </summary>
    public string? GuestEmail { get; init; }

    /// <summary>
    /// Misafir telefon.
    /// </summary>
    public string GuestPhone { get; init; } = string.Empty;

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati (UTC).
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Hedef masa ID (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Hedef masa birleşimi ID (opsiyonel).
    /// </summary>
    public Guid? TableCombinationId { get; init; }

    /// <summary>
    /// Özel istekler.
    /// </summary>
    public string? SpecialRequests { get; init; }

    /// <summary>
    /// Custom field yanıtları.
    /// </summary>
    public Dictionary<string, string>? CustomFieldAnswers { get; init; }
}
