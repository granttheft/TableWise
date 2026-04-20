using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Abonelik entity. Tenant'ın plan abonelik geçmişini tutar.
/// Her dönem için yeni bir kayıt oluşturulur (fatura için).
/// </summary>
public class Subscription : TenantScopedEntity
{
    /// <summary>
    /// Abonelik hangi plana ait (Foreign Key).
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Abonelik durumu (Trial, Active, PastDue, Suspended, Cancelled).
    /// </summary>
    public PlanStatus Status { get; set; }

    /// <summary>
    /// Abonelik dönem başlangıcı (UTC).
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Abonelik dönem bitişi (UTC).
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Ödeme tutarı.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Para birimi. Varsayılan: "TRY".
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// İyzico abonelik ID'si (recurring payment için).
    /// </summary>
    public string? IyzicoSubscriptionId { get; set; }

    /// <summary>
    /// İyzico müşteri ID'si.
    /// </summary>
    public string? IyzicoCustomerId { get; set; }

    /// <summary>
    /// Bir sonraki fatura tarihi (UTC).
    /// </summary>
    public DateTime? NextBillingDate { get; set; }

    /// <summary>
    /// İptal edilme tarihi (UTC). Null ise aktif.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Aboneliğin ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Abonelik planı.
    /// </summary>
    public virtual Plan? Plan { get; set; }
}
