using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Masa entity. Her masa bir mekana aittir ve kapasite, lokasyon gibi özelliklere sahiptir.
/// </summary>
public class Table : TenantScopedEntity
{
    /// <summary>
    /// Masa hangi mekana ait (Foreign Key).
    /// </summary>
    public Guid VenueId { get; set; }

    /// <summary>
    /// Masa adı. Örn: "Masa 1", "VIP 3", "Balkon 2".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Masa kapasitesi (kişi sayısı).
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Masa lokasyonu (Indoor, Outdoor, Balcony, vb.).
    /// </summary>
    public TableLocation Location { get; set; }

    /// <summary>
    /// Masa açıklaması (opsiyonel). Örn: "Deniz manzaralı", "Sessiz alan".
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sıralama önceliği. UI'da gösterim sırası için.
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Masa aktif mi? False ise rezervasyon kabul etmez.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Masanın ait olduğu mekan.
    /// </summary>
    public virtual Venue? Venue { get; set; }

    /// <summary>
    /// Masaya yapılan rezervasyonlar.
    /// </summary>
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
