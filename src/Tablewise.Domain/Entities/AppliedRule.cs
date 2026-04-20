using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Uygulanan kural entity. Rezervasyon sırasında hangi kuralların tetiklendiğini kaydeder.
/// BaseEntity'den türer (TenantScoped değil, Reservation üzerinden tenant'a erişilir).
/// </summary>
public class AppliedRule : BaseEntity
{
    /// <summary>
    /// Hangi rezervasyona uygulandı (Foreign Key).
    /// </summary>
    public Guid ReservationId { get; set; }

    /// <summary>
    /// Hangi kural tetiklendi (Foreign Key).
    /// </summary>
    public Guid RuleId { get; set; }

    /// <summary>
    /// Kural aksiyonu tipi (Allow, Block, Warn, Suggest, Discount, Deposit, Redirect).
    /// </summary>
    public RuleActionType ActionType { get; set; }

    /// <summary>
    /// Aksiyon payload (JSONB). Aksiyona özgü parametreler.
    /// Örn: { "discountPercent": 10 } veya { "suggestedSlots": [...] }
    /// </summary>
    public string? ActionPayload { get; set; }

    /// <summary>
    /// Kuralın değerlendirilme zamanı (UTC).
    /// </summary>
    public DateTime EvaluatedAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Kuralın uygulandığı rezervasyon.
    /// </summary>
    public virtual Reservation? Reservation { get; set; }

    /// <summary>
    /// Uygulanan kural.
    /// </summary>
    public virtual Rule? Rule { get; set; }
}
