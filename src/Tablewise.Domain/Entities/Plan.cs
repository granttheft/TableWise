using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Plan entity. Abonelik planlarını tanımlar (Starter, Pro, Business, Enterprise).
/// BaseEntity'den türer (TenantScoped değil, sistem geneli).
/// DB'de saklanır - deploy gerektirmeden plan değişiklikleri yapılabilir.
/// </summary>
public class Plan : BaseEntity
{
    /// <summary>
    /// Plan adı. Örn: "Starter", "Pro", "Business", "Enterprise".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Plan açıklaması.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Plan seviyesi (Starter, Pro, Business, Enterprise).
    /// </summary>
    public PlanTier Tier { get; set; }

    /// <summary>
    /// Aylık fiyat (TRY).
    /// </summary>
    public decimal MonthlyPriceTry { get; set; }

    /// <summary>
    /// Yıllık fiyat (TRY).
    /// </summary>
    public decimal YearlyPriceTry { get; set; }

    /// <summary>
    /// Plan özellikleri (JSONB). Feature flag'ler.
    /// Format: { "smsEnabled": true, "apiEnabled": false, "signalREnabled": true }
    /// </summary>
    public string FeaturesJson { get; set; } = "{}";

    /// <summary>
    /// Plan limitleri (JSONB).
    /// Format: { "maxVenues": 1, "maxTables": 3, "maxRules": 5, "maxReservationsPerMonth": 100 }
    /// </summary>
    public string LimitsJson { get; set; } = "{}";

    /// <summary>
    /// Plan görünür mü? False ise signup/upgrade sayfalarında gösterilmez.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Bu planı kullanan tenant'lar.
    /// </summary>
    public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();

    /// <summary>
    /// Bu plana ait abonelikler.
    /// </summary>
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
