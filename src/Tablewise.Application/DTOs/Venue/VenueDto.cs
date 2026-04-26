using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Venue;

/// <summary>
/// Venue (mekan) detay DTO'su.
/// </summary>
public sealed record VenueDto
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Mekan adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Mekan adresi.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Mekan telefon numarası.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Mekan açıklaması.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Mekan saat dilimi.
    /// </summary>
    public required string TimeZone { get; init; }

    /// <summary>
    /// Mekan logosu URL.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Slot süresi (dakika).
    /// </summary>
    public required int SlotDurationMinutes { get; init; }

    /// <summary>
    /// Kapora modülü aktif mi?
    /// </summary>
    public required bool DepositEnabled { get; init; }

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Kapora kişi başı mı?
    /// </summary>
    public required bool DepositPerPerson { get; init; }

    /// <summary>
    /// Kapora iade politikası.
    /// </summary>
    public required DepositRefundPolicy DepositRefundPolicy { get; init; }

    /// <summary>
    /// İade için minimum saat.
    /// </summary>
    public int? DepositRefundHours { get; init; }

    /// <summary>
    /// Kısmi iade yüzdesi.
    /// </summary>
    public decimal? DepositPartialPercent { get; init; }

    /// <summary>
    /// Çalışma saatleri (JSON).
    /// </summary>
    public string? WorkingHours { get; init; }

    /// <summary>
    /// Toplam masa sayısı.
    /// </summary>
    public int TableCount { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Güncellenme tarihi.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
