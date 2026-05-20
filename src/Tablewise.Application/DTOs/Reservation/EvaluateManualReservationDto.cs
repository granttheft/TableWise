using System.ComponentModel.DataAnnotations;

namespace Tablewise.Application.DTOs.Reservation;

/// <summary>
/// Manuel rezervasyon öncesi kural/slot değerlendirme isteği.
/// </summary>
public sealed record EvaluateManualReservationRequestDto
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
    /// Kişi sayısı.
    /// </summary>
    [Range(1, 50)]
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati.
    /// </summary>
    [Required(ErrorMessage = "Tarih ve saat zorunludur.")]
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Kayıtlı müşteri ID (opsiyonel).
    /// </summary>
    public Guid? CustomerId { get; init; }

    /// <summary>
    /// Misafir email (opsiyonel).
    /// </summary>
    [EmailAddress]
    public string? GuestEmail { get; init; }

    /// <summary>
    /// Misafir telefon (opsiyonel).
    /// </summary>
    public string? GuestPhone { get; init; }
}

/// <summary>
/// Değerlendirme mesajı (uyarı veya engel).
/// </summary>
public sealed record ReservationEvaluationItemDto
{
    /// <summary>
    /// Kullanıcıya gösterilecek mesaj.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// İlgili kural ID (yoksa boş).
    /// </summary>
    public string RuleId { get; init; } = string.Empty;
}

/// <summary>
/// Manuel rezervasyon kural değerlendirme yanıtı.
/// </summary>
public sealed record EvaluateManualReservationResponseDto
{
    /// <summary>
    /// Rezervasyon oluşturulabilir mi?
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Uyarılar (engel değil).
    /// </summary>
    public IReadOnlyList<ReservationEvaluationItemDto> Warnings { get; init; } = [];

    /// <summary>
    /// Engelleyici mesajlar.
    /// </summary>
    public IReadOnlyList<ReservationEvaluationItemDto> Blockers { get; init; } = [];

    /// <summary>
    /// Önerilen indirim yüzdesi.
    /// </summary>
    public decimal? DiscountPercent { get; init; }

    /// <summary>
    /// Kapora gerekli mi?
    /// </summary>
    public bool DepositRequired { get; init; }

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Uygulanan kural adları (özet).
    /// </summary>
    public IReadOnlyList<string> AppliedRules { get; init; } = [];
}
