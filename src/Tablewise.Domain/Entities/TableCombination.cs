using Tablewise.Domain.Common;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Masa birleşimi entity. Birden fazla masanın birleştirilerek daha büyük kapasite oluşturması.
/// Örn: "Masa 3 + Masa 4" birleşimi 12 kişilik grup için.
/// </summary>
public class TableCombination : TenantScopedEntity
{
    /// <summary>
    /// Birleşim hangi mekana ait (Foreign Key).
    /// </summary>
    public Guid VenueId { get; set; }

    /// <summary>
    /// Birleşim adı. Örn: "Masa 3+4 Birleşik", "VIP Alan".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Birleştirilen masa ID'leri (JSONB). Format: ["guid1", "guid2", "guid3"].
    /// </summary>
    public string TableIds { get; set; } = "[]";

    /// <summary>
    /// Birleşik toplam kapasite. Birleştirilen masaların kapasitelerinin toplamı.
    /// </summary>
    public int CombinedCapacity { get; set; }

    /// <summary>
    /// Birleşim aktif mi? False ise rezervasyon kabul etmez.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Birleşimin ait olduğu mekan.
    /// </summary>
    public virtual Venue? Venue { get; set; }

    /// <summary>
    /// Bu birleşimle yapılan rezervasyonlar.
    /// </summary>
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
