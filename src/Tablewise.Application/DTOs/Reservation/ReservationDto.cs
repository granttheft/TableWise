namespace Tablewise.Application.DTOs.Reservation;

/// <summary>
/// Rezervasyon liste/detay DTO (staff/owner için).
/// </summary>
public sealed record ReservationDto
{
    /// <summary>
    /// Rezervasyon ID.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Mekan ID.
    /// </summary>
    public Guid VenueId { get; init; }

    /// <summary>
    /// Mekan adı.
    /// </summary>
    public string VenueName { get; init; } = string.Empty;

    /// <summary>
    /// Masa ID.
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Masa adı.
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Masa birleşimi ID.
    /// </summary>
    public Guid? TableCombinationId { get; init; }

    /// <summary>
    /// Masa birleşimi adı.
    /// </summary>
    public string? TableCombinationName { get; init; }

    /// <summary>
    /// Müşteri ID.
    /// </summary>
    public Guid? CustomerId { get; init; }

    /// <summary>
    /// Misafir adı.
    /// </summary>
    public string GuestName { get; init; } = string.Empty;

    /// <summary>
    /// Misafir email.
    /// </summary>
    public string? GuestEmail { get; init; }

    /// <summary>
    /// Misafir telefon.
    /// </summary>
    public string GuestPhone { get; init; } = string.Empty;

    /// <summary>
    /// Müşteri tier.
    /// </summary>
    public string? CustomerTier { get; init; }

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati (UTC).
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Bitiş zamanı (UTC).
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Rezervasyon durumu.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Rezervasyon kaynağı.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Onay kodu.
    /// </summary>
    public string ConfirmCode { get; init; } = string.Empty;

    /// <summary>
    /// Özel istekler.
    /// </summary>
    public string? SpecialRequests { get; init; }

    /// <summary>
    /// Internal notlar.
    /// </summary>
    public string? InternalNotes { get; init; }

    /// <summary>
    /// İndirim yüzdesi.
    /// </summary>
    public decimal? DiscountPercent { get; init; }

    /// <summary>
    /// Kapora durumu.
    /// </summary>
    public string DepositStatus { get; init; } = string.Empty;

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Kapora ödenme tarihi.
    /// </summary>
    public DateTime? DepositPaidAt { get; init; }

    /// <summary>
    /// İptal nedeni.
    /// </summary>
    public string? CancellationReason { get; init; }

    /// <summary>
    /// İptal tarihi.
    /// </summary>
    public DateTime? CancelledAt { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Custom field yanıtları.
    /// </summary>
    public Dictionary<string, string>? CustomFieldAnswers { get; init; }

    /// <summary>
    /// Değiştirilmiş orijinal rezervasyon ID.
    /// </summary>
    public Guid? ModifiedFromReservationId { get; init; }
}

/// <summary>
/// Rezervasyon liste sorgu parametreleri.
/// </summary>
public sealed record ReservationListQueryDto
{
    /// <summary>
    /// Mekan ID filtresi.
    /// </summary>
    public Guid? VenueId { get; init; }

    /// <summary>
    /// Başlangıç tarihi filtresi.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Bitiş tarihi filtresi.
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// Durum filtresi (virgülle ayrılmış).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Arama terimi (isim, telefon, email, confirm code).
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Masa ID filtresi.
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Sayfa numarası (1-based).
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Sayfa boyutu.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Sıralama alanı.
    /// </summary>
    public string SortBy { get; init; } = "ReservedFor";

    /// <summary>
    /// Sıralama yönü (asc/desc).
    /// </summary>
    public string SortDirection { get; init; } = "asc";
}

/// <summary>
/// Paginated rezervasyon listesi response.
/// </summary>
public sealed record ReservationListResponseDto
{
    /// <summary>
    /// Rezervasyonlar.
    /// </summary>
    public IReadOnlyList<ReservationDto> Items { get; init; } = [];

    /// <summary>
    /// Toplam kayıt sayısı.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Sayfa numarası.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Sayfa boyutu.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Toplam sayfa sayısı.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
