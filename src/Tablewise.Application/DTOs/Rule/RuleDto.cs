using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Rule;

/// <summary>
/// Kural detay DTO.
/// </summary>
public sealed record RuleDto
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Mekan ID (nullable, null ise tenant geneli).
    /// </summary>
    public Guid? VenueId { get; init; }

    /// <summary>
    /// Mekan adı.
    /// </summary>
    public string? VenueName { get; init; }

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
    /// Koşullar JSON.
    /// </summary>
    public string ConditionsJson { get; init; } = string.Empty;

    /// <summary>
    /// Aksiyonlar JSON.
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

    /// <summary>
    /// Kaç kez tetiklendiği.
    /// </summary>
    public int TimesTriggered { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Güncellenme tarihi.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
