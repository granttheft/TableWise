using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kural güncelleme DTO.
/// </summary>
public sealed record UpdateRuleDto
{
    /// <summary>
    /// Kural adı.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Kural açıklaması.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Kural tipi.
    /// </summary>
    public string RuleType { get; init; } = string.Empty;

    /// <summary>
    /// Koşullar JSON (version alanı zorunlu).
    /// </summary>
    public string ConditionsJson { get; init; } = string.Empty;

    /// <summary>
    /// Aksiyonlar JSON (version alanı zorunlu).
    /// </summary>
    public string ActionsJson { get; init; } = string.Empty;

    /// <summary>
    /// Öncelik sırası (1 = en yüksek).
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Tetikleyici tip.
    /// </summary>
    public RuleTrigger TriggerType { get; init; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Uygulanabilir zaman dilimleri JSON.
    /// </summary>
    public string? ApplicableTimeSlots { get; init; }
}
