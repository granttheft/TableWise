using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Venue;

/// <summary>
/// Venue güncelleme DTO'su.
/// </summary>
public sealed record UpdateVenueDto
{
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
    /// Çalışma saatleri (JSON formatında).
    /// </summary>
    public string? WorkingHours { get; init; }
}
