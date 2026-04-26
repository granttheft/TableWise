using System.ComponentModel.DataAnnotations;

namespace Tablewise.Application.DTOs.Booking;

/// <summary>
/// Rezervasyon oluşturma istek DTO.
/// Public booking endpoint için.
/// </summary>
public sealed record ReserveRequestDto
{
    /// <summary>
    /// Misafir adı soyadı.
    /// </summary>
    [Required(ErrorMessage = "İsim soyisim zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim 2-100 karakter arasında olmalıdır.")]
    public string GuestName { get; init; } = string.Empty;

    /// <summary>
    /// Misafir email.
    /// </summary>
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [StringLength(255)]
    public string? GuestEmail { get; init; }

    /// <summary>
    /// Misafir telefon (zorunlu).
    /// </summary>
    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string GuestPhone { get; init; } = string.Empty;

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    [Required(ErrorMessage = "Kişi sayısı zorunludur.")]
    [Range(1, 50, ErrorMessage = "Kişi sayısı 1-50 arasında olmalıdır.")]
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi ve saati (ISO 8601 UTC).
    /// </summary>
    [Required(ErrorMessage = "Tarih ve saat zorunludur.")]
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Hedef masa ID (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Hedef masa birleşimi ID (opsiyonel).
    /// </summary>
    public Guid? TableCombinationId { get; init; }

    /// <summary>
    /// Özel istekler (opsiyonel).
    /// </summary>
    [StringLength(500, ErrorMessage = "Özel istekler 500 karakteri geçemez.")]
    public string? SpecialRequests { get; init; }

    /// <summary>
    /// Custom field yanıtları (JSONB formatında).
    /// Key: FieldId, Value: Yanıt.
    /// </summary>
    public Dictionary<string, string>? CustomFieldAnswers { get; init; }

    /// <summary>
    /// KVKK onayı.
    /// </summary>
    [Required(ErrorMessage = "Gizlilik politikası onayı zorunludur.")]
    public bool PrivacyPolicyAccepted { get; init; }
}

/// <summary>
/// Rezervasyon response DTO.
/// </summary>
public sealed record ReserveResponseDto
{
    /// <summary>
    /// Rezervasyon ID.
    /// </summary>
    public Guid ReservationId { get; init; }

    /// <summary>
    /// Onay kodu (8 karakter).
    /// </summary>
    public string ConfirmCode { get; init; } = string.Empty;

    /// <summary>
    /// Rezervasyon durumu.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Rezervasyon tarihi/saati (UTC).
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Bitiş zamanı (UTC).
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Mekan adı.
    /// </summary>
    public string VenueName { get; init; } = string.Empty;

    /// <summary>
    /// Masa adı.
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Kapora gerekli mi?
    /// </summary>
    public bool DepositRequired { get; init; }

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Ödeme URL (kapora gerekli ise).
    /// </summary>
    public string? PaymentUrl { get; init; }

    /// <summary>
    /// Uygulanan indirim yüzdesi.
    /// </summary>
    public decimal? DiscountPercent { get; init; }

    /// <summary>
    /// Uyarı mesajları.
    /// </summary>
    public IReadOnlyList<string>? Warnings { get; init; }
}
