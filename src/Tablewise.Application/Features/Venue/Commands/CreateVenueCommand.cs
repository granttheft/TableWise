using MediatR;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue oluşturma komutu.
/// Sadece Owner rolü kullanabilir.
/// Plan limitlerini kontrol eder.
/// </summary>
public sealed record CreateVenueCommand : IRequest<Guid>
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
    /// Çalışma saatleri (JSON).
    /// </summary>
    public string? WorkingHours { get; init; }
}
