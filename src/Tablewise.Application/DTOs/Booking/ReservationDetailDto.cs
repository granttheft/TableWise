namespace Tablewise.Application.DTOs.Booking;

/// <summary>
/// Rezervasyon detay DTO (public endpoint için).
/// </summary>
public sealed record ReservationDetailDto
{
    /// <summary>
    /// Onay kodu.
    /// </summary>
    public string ConfirmCode { get; init; } = string.Empty;

    /// <summary>
    /// Rezervasyon durumu.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Misafir adı.
    /// </summary>
    public string GuestName { get; init; } = string.Empty;

    /// <summary>
    /// Rezervasyon tarihi/saati (UTC).
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Bitiş zamanı (UTC).
    /// </summary>
    public DateTime EndTime { get; init; }

    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Mekan adı.
    /// </summary>
    public string VenueName { get; init; } = string.Empty;

    /// <summary>
    /// Mekan adresi.
    /// </summary>
    public string? VenueAddress { get; init; }

    /// <summary>
    /// Mekan telefonu.
    /// </summary>
    public string? VenuePhone { get; init; }

    /// <summary>
    /// Masa adı.
    /// </summary>
    public string? TableName { get; init; }

    /// <summary>
    /// Özel istekler.
    /// </summary>
    public string? SpecialRequests { get; init; }

    /// <summary>
    /// Kapora durumu.
    /// </summary>
    public string? DepositStatus { get; init; }

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Değiştirme mümkün mü? (24 saat öncesine kadar).
    /// </summary>
    public bool CanModify { get; init; }

    /// <summary>
    /// İptal mümkün mü? (24 saat öncesine kadar).
    /// </summary>
    public bool CanCancel { get; init; }

    /// <summary>
    /// Değiştirme/iptal için kalan süre (saat).
    /// </summary>
    public int? HoursUntilDeadline { get; init; }
}

/// <summary>
/// Rezervasyon iptal istek DTO.
/// </summary>
public sealed record CancelReservationRequestDto
{
    /// <summary>
    /// İptal nedeni (opsiyonel).
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Rezervasyon değiştirme istek DTO.
/// </summary>
public sealed record ModifyReservationRequestDto
{
    /// <summary>
    /// Yeni tarih/saat (opsiyonel).
    /// </summary>
    public DateTime? NewDateTime { get; init; }

    /// <summary>
    /// Yeni masa ID (opsiyonel).
    /// </summary>
    public Guid? NewTableId { get; init; }

    /// <summary>
    /// Yeni kişi sayısı (opsiyonel).
    /// </summary>
    public int? NewPartySize { get; init; }
}

/// <summary>
/// Kural ön izleme istek DTO.
/// </summary>
public sealed record EvaluateRulesRequestDto
{
    /// <summary>
    /// Kişi sayısı.
    /// </summary>
    public int PartySize { get; init; }

    /// <summary>
    /// Rezervasyon tarihi/saati.
    /// </summary>
    public DateTime ReservedFor { get; init; }

    /// <summary>
    /// Masa ID (opsiyonel).
    /// </summary>
    public Guid? TableId { get; init; }

    /// <summary>
    /// Müşteri email (opsiyonel).
    /// </summary>
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// Müşteri telefon (opsiyonel).
    /// </summary>
    public string? CustomerPhone { get; init; }
}

/// <summary>
/// Kural ön izleme response DTO.
/// </summary>
public sealed record EvaluateRulesResponseDto
{
    /// <summary>
    /// İstek kabul edilir mi?
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Engelleme nedeni.
    /// </summary>
    public string? BlockReason { get; init; }

    /// <summary>
    /// Uygulanacak indirim.
    /// </summary>
    public decimal? DiscountPercent { get; init; }

    /// <summary>
    /// Kapora gerekli mi?
    /// </summary>
    public bool RequiresDeposit { get; init; }

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Uyarılar.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
