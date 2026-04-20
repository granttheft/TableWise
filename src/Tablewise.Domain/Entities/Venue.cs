using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Mekan entity. Tenant altında birden fazla mekan olabilir (Business+ planlarda).
/// Her mekanın kendine özgü ayarları, masaları ve rezervasyonları vardır.
/// </summary>
public class Venue : TenantScopedEntity
{
    /// <summary>
    /// Mekan adı.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mekan adresi (opsiyonel).
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Mekan saat dilimi. Varsayılan: "Europe/Istanbul".
    /// </summary>
    public string TimeZone { get; set; } = "Europe/Istanbul";

    /// <summary>
    /// Mekan açılış saati (TimeSpan formatında).
    /// </summary>
    public TimeSpan OpeningTime { get; set; }

    /// <summary>
    /// Mekan kapanış saati (TimeSpan formatında).
    /// </summary>
    public TimeSpan ClosingTime { get; set; }

    /// <summary>
    /// Slot süresi (dakika). Varsayılan: 90 dakika.
    /// </summary>
    public int SlotDurationMinutes { get; set; } = 90;

    /// <summary>
    /// Haftalık çalışma saatleri yapılandırması (JSONB).
    /// Format: { "Monday": { "open": "10:00", "close": "22:00" }, ... }
    /// </summary>
    public string? WorkingHours { get; set; }

    /// <summary>
    /// Mekan logosu URL (Cloudflare R2).
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Kapora modülü aktif mi? Pro+ plan gerektirir.
    /// </summary>
    public bool DepositEnabled { get; set; } = false;

    /// <summary>
    /// Kapora tutarı (sabit miktar veya kişi başı).
    /// </summary>
    public decimal? DepositAmount { get; set; }

    /// <summary>
    /// Kapora kişi başı mı hesaplansın?
    /// </summary>
    public bool DepositPerPerson { get; set; } = false;

    /// <summary>
    /// Kapora iade politikası.
    /// </summary>
    public DepositRefundPolicy DepositRefundPolicy { get; set; }

    /// <summary>
    /// İade için minimum kaç saat öncesinden iptal gerekli.
    /// </summary>
    public int? DepositRefundHours { get; set; }

    /// <summary>
    /// Kısmi iade yüzdesi (DepositRefundPolicy = PartialRefund ise).
    /// </summary>
    public decimal? DepositPartialPercent { get; set; }

    /// <summary>
    /// Mekan telefon numarası.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Mekan açıklaması.
    /// </summary>
    public string? Description { get; set; }

    // Navigation Properties

    /// <summary>
    /// Mekan'ın ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Mekan'a ait masalar.
    /// </summary>
    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();

    /// <summary>
    /// Mekan'a ait kurallar.
    /// </summary>
    public virtual ICollection<Rule> Rules { get; set; } = new List<Rule>();

    /// <summary>
    /// Mekan'a ait rezervasyonlar.
    /// </summary>
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    /// <summary>
    /// Mekan'ın kapalı olduğu günler/saatler.
    /// </summary>
    public virtual ICollection<VenueClosure> Closures { get; set; } = new List<VenueClosure>();

    /// <summary>
    /// Mekan'a özgü custom field'lar (rezervasyon formunda ekstra alanlar).
    /// </summary>
    public virtual ICollection<VenueCustomField> CustomFields { get; set; } = new List<VenueCustomField>();

    /// <summary>
    /// Mekan'a ait masa birleşimleri.
    /// </summary>
    public virtual ICollection<TableCombination> TableCombinations { get; set; } = new List<TableCombination>();
}
