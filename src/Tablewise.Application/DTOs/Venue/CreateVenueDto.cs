using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Venue;

/// <summary>
/// Venue oluşturma DTO'su.
/// </summary>
public sealed record CreateVenueDto
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
    /// Mekan saat dilimi. Varsayılan: "Europe/Istanbul".
    /// </summary>
    public string TimeZone { get; init; } = "Europe/Istanbul";

    /// <summary>
    /// Slot süresi (dakika). Varsayılan: 90.
    /// </summary>
    public int SlotDurationMinutes { get; init; } = 90;

    /// <summary>
    /// Kapora modülü aktif mi?
    /// </summary>
    public bool DepositEnabled { get; init; } = false;

    /// <summary>
    /// Kapora tutarı.
    /// </summary>
    public decimal? DepositAmount { get; init; }

    /// <summary>
    /// Kapora kişi başı mı?
    /// </summary>
    public bool DepositPerPerson { get; init; } = false;

    /// <summary>
    /// Kapora iade politikası.
    /// </summary>
    public DepositRefundPolicy DepositRefundPolicy { get; init; } = DepositRefundPolicy.NoRefund;

    /// <summary>
    /// İade için minimum saat.
    /// </summary>
    public int? DepositRefundHours { get; init; }

    /// <summary>
    /// Kısmi iade yüzdesi (0-100 arası).
    /// </summary>
    public decimal? DepositPartialPercent { get; init; }

    /// <summary>
    /// Çalışma saatleri (JSON formatında).
    /// </summary>
    public string? WorkingHours { get; init; }
}
