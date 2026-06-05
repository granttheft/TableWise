using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.WhatsApp;

/// <summary>
/// WhatsApp mesaj geçmişi liste DTO'su.
/// </summary>
public sealed record WhatsAppMessageHistoryDto
{
    /// <summary>
    /// Mesaj ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Alıcı telefon (maskelenmiş).
    /// </summary>
    public required string ToPhone { get; init; }

    /// <summary>
    /// Kullanılan şablon.
    /// </summary>
    public required WhatsAppMessageTemplate Template { get; init; }

    /// <summary>
    /// Mesaj durumu.
    /// </summary>
    public required WhatsAppMessageStatus Status { get; init; }

    /// <summary>
    /// Gönderim zamanı (UTC).
    /// </summary>
    public DateTime? SentAt { get; init; }

    /// <summary>
    /// Teslim zamanı (UTC).
    /// </summary>
    public DateTime? DeliveredAt { get; init; }

    /// <summary>
    /// Hata mesajı (başarısız gönderimde).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// İlişkili rezervasyon ID.
    /// </summary>
    public Guid? ReservationId { get; init; }
}
