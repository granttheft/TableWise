using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Müşteri entity. Rezervasyon yapan kişiler (misafirler).
/// CRM ve segmentasyon için kullanılır. Phone + TenantId unique.
/// </summary>
public class Customer : TenantScopedEntity
{
    /// <summary>
    /// Müşteri tam adı.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Müşteri telefon numarası. TenantId ile birlikte unique olmalı.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Müşteri email adresi (opsiyonel).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Müşteri segmenti (Regular, Gold, VIP, Blacklisted).
    /// </summary>
    public CustomerTier Tier { get; set; } = CustomerTier.Regular;

    /// <summary>
    /// Müşteri kara listede mi?
    /// </summary>
    public bool IsBlacklisted { get; set; } = false;

    /// <summary>
    /// Kara listeye alınma nedeni. IsBlacklisted=true ise zorunlu.
    /// </summary>
    public string? BlacklistReason { get; set; }

    /// <summary>
    /// Müşteri hakkında notlar (opsiyonel). KVKK uyumlu, hassas bilgi içermemeli.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Toplam ziyaret sayısı (completed rezervasyon).
    /// </summary>
    public int TotalVisits { get; set; } = 0;

    /// <summary>
    /// Son rezervasyon tarihi (UTC).
    /// </summary>
    public DateTime? LastReservationAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Müşterinin ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Müşterinin yaptığı rezervasyonlar.
    /// </summary>
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
