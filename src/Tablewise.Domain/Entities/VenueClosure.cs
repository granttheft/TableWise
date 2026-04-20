using Tablewise.Domain.Common;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Mekan kapalılık entity. Belirli günlerde veya saatlerde mekanın kapalı olduğunu tanımlar.
/// Slot hesaplama yapılırken mutlaka kontrol edilmelidir.
/// </summary>
public class VenueClosure : TenantScopedEntity
{
    /// <summary>
    /// Kapalılık hangi mekana ait (Foreign Key).
    /// </summary>
    public Guid VenueId { get; set; }

    /// <summary>
    /// Kapalılık tarihi. Aynı VenueId + Date unique olmalı (index).
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Tüm gün kapalı mı?
    /// </summary>
    public bool IsFullDay { get; set; }

    /// <summary>
    /// Kısmi kapalılık için açılış saati (TimeSpan). IsFullDay=false ise kullanılır.
    /// </summary>
    public TimeSpan? OpenTime { get; set; }

    /// <summary>
    /// Kısmi kapalılık için kapanış saati (TimeSpan). IsFullDay=false ise kullanılır.
    /// </summary>
    public TimeSpan? CloseTime { get; set; }

    /// <summary>
    /// Kapalılık nedeni (opsiyonel). Örn: "Yılbaşı tatili", "Özel etkinlik".
    /// </summary>
    public string? Reason { get; set; }

    // Navigation Properties

    /// <summary>
    /// Kapalılığın ait olduğu mekan.
    /// </summary>
    public virtual Venue? Venue { get; set; }
}
