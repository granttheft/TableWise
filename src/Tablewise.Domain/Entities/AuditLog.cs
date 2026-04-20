using Tablewise.Domain.Common;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Audit log entity. Sistem genelinde yapılan işlemleri kaydeder (audit trail).
/// KVKK uyumu için hassas bilgi loglanmamalı.
/// </summary>
public class AuditLog : TenantScopedEntity
{
    /// <summary>
    /// İşlemi yapan kullanıcı ID'si (nullable). Sistem otomatik ise null.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// İşlemi yapan (kullanıcı email veya "Sistem").
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Yapılan işlem. Örn: "RULE_CREATED", "RESERVATION_CONFIRMED", "USER_INVITED".
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// İşlem yapılan entity tipi. Örn: "Rule", "Reservation", "User".
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// İşlem yapılan entity ID'si (string olarak, Guid veya başka tip olabilir).
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Eski değer (JSONB, nullable). Update işlemlerinde önceki değer.
    /// KVKK: Hassas bilgi içermemeli.
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Yeni değer (JSONB, nullable). Update işlemlerinde yeni değer.
    /// KVKK: Hassas bilgi içermemeli.
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// İşlemi yapan IP adresi (opsiyonel).
    /// </summary>
    public string? IpAddress { get; set; }

    // Navigation Properties

    /// <summary>
    /// Audit log'un ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// İşlemi yapan kullanıcı.
    /// </summary>
    public virtual User? User { get; set; }
}
