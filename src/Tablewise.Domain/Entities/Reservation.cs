using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Rezervasyon entity. Sistemin ana iş nesnesi.
/// Müşteri, masa, slot, kapora, kural uygulamaları gibi tüm bilgileri içerir.
/// </summary>
public class Reservation : TenantScopedEntity
{
    /// <summary>
    /// Rezervasyon hangi mekana ait (Foreign Key).
    /// </summary>
    public Guid VenueId { get; set; }

    /// <summary>
    /// Rezervasyon hangi masaya ait (Foreign Key). Null olabilir (henüz atanmamış).
    /// </summary>
    public Guid? TableId { get; set; }

    /// <summary>
    /// Masa birleşimi kullanıldıysa (Foreign Key, nullable).
    /// </summary>
    public Guid? TableCombinationId { get; set; }

    /// <summary>
    /// Müşteri ID (Foreign Key, nullable). Kayıtlı müşteri ise dolu.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Misafir adı. CustomerId null ise zorunlu.
    /// </summary>
    public string GuestName { get; set; } = string.Empty;

    /// <summary>
    /// Misafir email (opsiyonel).
    /// </summary>
    public string? GuestEmail { get; set; }

    /// <summary>
    /// Misafir telefon. Zorunlu (müşteri iletişimi için).
    /// </summary>
    public string GuestPhone { get; set; } = string.Empty;

    /// <summary>
    /// Kişi sayısı (party size).
    /// </summary>
    public int PartySize { get; set; }

    /// <summary>
    /// Rezervasyon tarihi ve saati (UTC).
    /// </summary>
    public DateTime ReservedFor { get; set; }

    /// <summary>
    /// Rezervasyon bitiş zamanı (UTC). ReservedFor + SlotDurationMinutes.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Rezervasyon durumu (Pending, Confirmed, Completed, Cancelled, NoShow).
    /// </summary>
    public ReservationStatus Status { get; set; }

    /// <summary>
    /// Rezervasyon kaynağı (BookingUI, ManualAdmin, ManualStaff, Api, Whatsapp).
    /// </summary>
    public ReservationSource Source { get; set; }

    /// <summary>
    /// Onay kodu. 8 karakter alphanumeric, kriptografik güvenli random.
    /// Benzersiz olmalı.
    /// </summary>
    public string ConfirmCode { get; set; } = string.Empty;

    /// <summary>
    /// Müşteriden özel istekler (opsiyonel). KVKK uyumlu, hassas bilgi içermemeli.
    /// </summary>
    public string? SpecialRequests { get; set; }

    /// <summary>
    /// Mekan personeli notları (opsiyonel). Müşteriye gösterilmez.
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Uygulanan indirim yüzdesi (kural motoru tarafından set edilebilir).
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Runtime'da uygulanan kuralların snapshot'ı (JSONB).
    /// Hangi kurallar tetiklendi ve ne yapıldı.
    /// </summary>
    public string? AppliedRulesSnapshot { get; set; }

    /// <summary>
    /// VenueCustomField yanıtları (JSONB). Format: { "fieldId": "value" }.
    /// </summary>
    public string? CustomFieldAnswers { get; set; }

    /// <summary>
    /// Kapora durumu (NotRequired, Pending, Paid, Refunded, Forfeited, Failed).
    /// </summary>
    public DepositStatus DepositStatus { get; set; } = DepositStatus.NotRequired;

    /// <summary>
    /// Kapora tutarı (ödenmesi gereken veya ödenen).
    /// </summary>
    public decimal? DepositAmount { get; set; }

    /// <summary>
    /// İyzico ödeme referansı.
    /// </summary>
    public string? DepositPaymentRef { get; set; }

    /// <summary>
    /// Kapora ödenme tarihi (UTC).
    /// </summary>
    public DateTime? DepositPaidAt { get; set; }

    /// <summary>
    /// Kapora iade tarihi (UTC).
    /// </summary>
    public DateTime? DepositRefundedAt { get; set; }

    /// <summary>
    /// İptal nedeni (opsiyonel). Status=Cancelled ise dolu olabilir.
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// İptal tarihi (UTC).
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Hatırlatma bildirimi gönderilme tarihi (UTC).
    /// </summary>
    public DateTime? ReminderSentAt { get; set; }

    /// <summary>
    /// Eğer rezervasyon değiştirme ile oluştuysa, orijinal rezervasyon ID'si.
    /// </summary>
    public Guid? ModifiedFromReservationId { get; set; }

    // Navigation Properties

    /// <summary>
    /// Rezervasyonun ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Rezervasyonun yapıldığı mekan.
    /// </summary>
    public virtual Venue? Venue { get; set; }

    /// <summary>
    /// Rezervasyonun atandığı masa.
    /// </summary>
    public virtual Table? Table { get; set; }

    /// <summary>
    /// Rezervasyonun atandığı masa birleşimi.
    /// </summary>
    public virtual TableCombination? TableCombination { get; set; }

    /// <summary>
    /// Rezervasyonu yapan kayıtlı müşteri.
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Rezervasyonun orijinal hali (değiştirme durumunda).
    /// </summary>
    public virtual Reservation? ModifiedFromReservation { get; set; }

    /// <summary>
    /// Bu rezervasyondan değiştirilerek oluşturulan rezervasyonlar.
    /// </summary>
    public virtual ICollection<Reservation> ModifiedReservations { get; set; } = new List<Reservation>();

    /// <summary>
    /// Rezervasyona uygulanan kurallar.
    /// </summary>
    public virtual ICollection<AppliedRule> AppliedRules { get; set; } = new List<AppliedRule>();

    /// <summary>
    /// Rezervasyon durum değişiklik logları.
    /// </summary>
    public virtual ICollection<ReservationStatusLog> StatusLogs { get; set; } = new List<ReservationStatusLog>();

    /// <summary>
    /// Rezervasyonla ilgili bildirimler.
    /// </summary>
    public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
