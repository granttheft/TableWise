using System.ComponentModel.DataAnnotations;

namespace Tablewise.Application.DTOs.Reservation;

/// <summary>
/// Manuel rezervasyon oluşturma DTO (staff/owner için).
/// </summary>
public sealed record CreateReservationDto
{
    /// <summary>
    /// Mekan ID.
    /// </summary>
    [Required(ErrorMessage = "Mekan seçimi zorunludur.")]
    public Guid VenueId { get; init; }

    /// <summary>
    /// Masa ID (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Masa birleşimi ID (opsiyonel).
    /// </summary>
    public Guid? TableCombinationId { get; init; }

    /// <summary>
    /// Misafir adı.
    /// </summary>
    [Required(ErrorMessage = "Misafir adı zorunludur.")]
    [StringLength(100, MinimumLength = 2)]
    public string GuestName { get; init; } = string.Empty;

    /// <summary>
    /// Misafir email.
    /// </summary>
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public string? GuestEmail { get; init; }

    /// <summary>
    /// Misafir telefon.
    /// </summary>
    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone]
    public string GuestPhone { get; init; } = string.Empty;

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    [Required]
    [Range(1, 50)]
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati (UTC).
    /// </summary>
    [Required(ErrorMessage = "Tarih ve saat zorunludur.")]
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Özel istekler.
    /// </summary>
    [StringLength(500)]
    public string? SpecialRequests { get; init; }

    /// <summary>
    /// Internal not.
    /// </summary>
    [StringLength(1000)]
    public string? InternalNotes { get; init; }

    /// <summary>
    /// Kapora bypass (admin tarafından kapora gerektirmeden onay).
    /// </summary>
    public bool BypassDeposit { get; init; }

    /// <summary>
    /// Kural bypass (admin tarafından kuralları atlama).
    /// </summary>
    public bool BypassRules { get; init; }

    /// <summary>
    /// Onay email'i gönderilsin mi?
    /// </summary>
    public bool SendConfirmationEmail { get; init; } = true;
}

/// <summary>
/// Rezervasyon durum güncelleme DTO.
/// </summary>
public sealed record UpdateReservationStatusDto
{
    /// <summary>
    /// Yeni durum.
    /// </summary>
    [Required(ErrorMessage = "Durum zorunludur.")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Durum değişikliği nedeni (opsiyonel).
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; init; }
}

/// <summary>
/// Rezervasyon iptal DTO (işletme tarafından).
/// </summary>
public sealed record CancelReservationByStaffDto
{
    /// <summary>
    /// İptal nedeni.
    /// </summary>
    [Required(ErrorMessage = "İptal nedeni zorunludur.")]
    [StringLength(500)]
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// İptal bildirimi gönderilsin mi?
    /// </summary>
    public bool SendNotification { get; init; } = true;

    /// <summary>
    /// Kapora iadesi yapılsın mı?
    /// </summary>
    public bool RefundDeposit { get; init; } = true;
}

/// <summary>
/// Internal not ekleme DTO.
/// </summary>
public sealed record AddInternalNoteDto
{
    /// <summary>
    /// Not içeriği.
    /// </summary>
    [Required(ErrorMessage = "Not içeriği zorunludur.")]
    [StringLength(1000)]
    public string Note { get; init; } = string.Empty;
}
