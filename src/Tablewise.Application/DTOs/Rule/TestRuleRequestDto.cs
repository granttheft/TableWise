namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kural test istek DTO.
/// </summary>
public sealed record TestRuleRequestDto
{
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
