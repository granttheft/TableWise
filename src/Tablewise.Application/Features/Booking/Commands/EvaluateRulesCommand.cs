using MediatR;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Features.Booking.Commands;

/// <summary>
/// Kural ön izleme komutu. Kayıt oluşturmadan kuralları değerlendirir.
/// </summary>
public sealed record EvaluateRulesCommand : IRequest<EvaluateRulesResponseDto>
{
    /// <summary>
    /// Tenant slug.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati.
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Masa ID (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Müşteri email (opsiyonel).
    /// </summary>
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// Müşteri telefon (opsiyonel).
    /// </summary>
    public string? CustomerPhone { get; init; }
}
