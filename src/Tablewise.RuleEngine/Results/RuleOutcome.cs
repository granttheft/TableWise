using Tablewise.Domain.Enums;

namespace Tablewise.RuleEngine.Results;

/// <summary>
/// Tek bir kuralın değerlendirme sonucu.
/// </summary>
public sealed record RuleOutcome
{
    /// <summary>
    /// Kuralın ID'si.
    /// </summary>
    public required Guid RuleId { get; init; }

    /// <summary>
    /// Kuralın adı (görüntüleme için).
    /// </summary>
    public required string RuleName { get; init; }

    /// <summary>
    /// Aksiyonun tipi.
    /// </summary>
    public required RuleActionType ActionType { get; init; }

    /// <summary>
    /// Kullanıcıya gösterilecek mesaj (opsiyonel).
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Aksiyona özgü payload (JSON, opsiyonel).
    /// Örn: {"discountPercent": 10} veya {"suggestedSlots": [...]}
    /// </summary>
    public string? Payload { get; init; }

    /// <summary>
    /// Kural önceliği (debug/logging için).
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Değerlendirme zamanı (UTC).
    /// </summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;
}
