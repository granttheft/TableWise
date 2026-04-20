using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Rezervasyon durum değişiklik log entity. Her status geçişini kaydeder (audit trail).
/// BaseEntity'den türer (TenantScoped değil, Reservation üzerinden tenant'a erişilir).
/// </summary>
public class ReservationStatusLog : BaseEntity
{
    /// <summary>
    /// Hangi rezervasyonun durumu değişti (Foreign Key).
    /// </summary>
    public Guid ReservationId { get; set; }

    /// <summary>
    /// Önceki durum.
    /// </summary>
    public ReservationStatus FromStatus { get; set; }

    /// <summary>
    /// Yeni durum.
    /// </summary>
    public ReservationStatus ToStatus { get; set; }

    /// <summary>
    /// Değişikliği yapan kullanıcı ID'si (nullable). Sistem otomatik ise null.
    /// </summary>
    public Guid? ChangedByUserId { get; set; }

    /// <summary>
    /// Değişikliği yapan (kullanıcı email veya "Sistem").
    /// </summary>
    public string? ChangedBy { get; set; }

    /// <summary>
    /// Değişiklik nedeni (opsiyonel).
    /// </summary>
    public string? Reason { get; set; }

    // Navigation Properties

    /// <summary>
    /// Durum değişikliğinin kaydedildiği rezervasyon.
    /// </summary>
    public virtual Reservation? Reservation { get; set; }

    /// <summary>
    /// Değişikliği yapan kullanıcı.
    /// </summary>
    public virtual User? ChangedByUser { get; set; }
}
