using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Kural entity. İş kurallarını tanımlar (kural motoru tarafından değerlendirilir).
/// Mekan seviyesinde veya tenant genelinde tanımlanabilir.
/// </summary>
public class Rule : TenantScopedEntity
{
    /// <summary>
    /// Kural hangi mekana özgü (Foreign Key, nullable). Null ise tenant geneli.
    /// </summary>
    public Guid? VenueId { get; set; }

    /// <summary>
    /// Kural adı. Örn: "Hafta sonu VIP önceliği", "Erken rezervasyon indirimi".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kural açıklaması (opsiyonel).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Kural tipi. Örn: "EarlyBooking", "VIPPriority", "CustomCondition".
    /// Kural motoru bu alana göre hangi evaluator'ı kullanacağını belirler.
    /// </summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>
    /// Kural koşulları (JSONB). "version" alanı içermeli (şema migration için).
    /// Format: { "version": 1, "conditions": [...] }
    /// </summary>
    public string ConditionsJson { get; set; } = "{}";

    /// <summary>
    /// Kural aksiyonları (JSONB). "version" alanı içermeli.
    /// Format: { "version": 1, "actions": [...] }
    /// </summary>
    public string ActionsJson { get; set; } = "{}";

    /// <summary>
    /// Kural önceliği. 1 = en yüksek. Aynı trigger'da birden fazla kural tetiklenirse önceliğe göre sıralanır.
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Kural tetikleyici (OnReservationCreate, OnSeatAssign, OnCancel).
    /// </summary>
    public RuleTrigger TriggerType { get; set; }

    /// <summary>
    /// Kural aktif mi? False ise değerlendirilmez.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Kuralın hangi zaman dilimlerinde geçerli olduğu (JSONB, nullable).
    /// Format: { "dayOfWeek": [1, 6, 7], "timeRanges": ["18:00-23:00"] }
    /// </summary>
    public string? ApplicableTimeSlots { get; set; }

    /// <summary>
    /// Kuralın kaç kez tetiklendiği (istatistik amaçlı).
    /// </summary>
    public int TimesTriggered { get; set; } = 0;

    // Navigation Properties

    /// <summary>
    /// Kuralın ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Kuralın özgü olduğu mekan (nullable, tenant geneli ise null).
    /// </summary>
    public virtual Venue? Venue { get; set; }

    /// <summary>
    /// Kuralın uygulandığı rezervasyonlar.
    /// </summary>
    public virtual ICollection<AppliedRule> AppliedRules { get; set; } = new List<AppliedRule>();
}
