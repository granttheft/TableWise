namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Tenant kullanım istatistikleri DTO'su.
/// Plan limitlerine göre mevcut kullanımı gösterir.
/// </summary>
public sealed record TenantUsageDto
{
    /// <summary>
    /// Bu ay yapılan rezervasyon sayısı.
    /// </summary>
    public required int ReservationsThisMonth { get; init; }

    /// <summary>
    /// Plan bazlı aylık rezervasyon limiti.
    /// </summary>
    public required int MonthlyReservationLimit { get; init; }

    /// <summary>
    /// Oluşturulmuş venue (mekan) sayısı.
    /// </summary>
    public required int VenueCount { get; init; }

    /// <summary>
    /// Plan bazlı venue limiti.
    /// </summary>
    public required int VenueLimit { get; init; }

    /// <summary>
    /// Toplam kullanıcı sayısı (Owner + Staff).
    /// </summary>
    public required int UserCount { get; init; }

    /// <summary>
    /// Plan bazlı kullanıcı limiti.
    /// </summary>
    public required int UserLimit { get; init; }

    /// <summary>
    /// Toplam masa sayısı (tüm venue'ler).
    /// </summary>
    public required int TableCount { get; init; }

    /// <summary>
    /// Plan bazlı masa limiti.
    /// </summary>
    public required int TableLimit { get; init; }

    /// <summary>
    /// Rezervasyon limiti doluluk oranı (0-1 arası).
    /// </summary>
    public decimal ReservationUsageRatio => MonthlyReservationLimit > 0 
        ? (decimal)ReservationsThisMonth / MonthlyReservationLimit 
        : 0;

    /// <summary>
    /// Rezervasyon limiti dolmak üzere mi? (%80 üzeri).
    /// </summary>
    public bool IsNearReservationLimit => ReservationUsageRatio >= 0.8m;

    /// <summary>
    /// Venue limiti doldu mu?
    /// </summary>
    public bool IsVenueLimitReached => VenueCount >= VenueLimit;

    /// <summary>
    /// Kullanıcı limiti doldu mu?
    /// </summary>
    public bool IsUserLimitReached => UserCount >= UserLimit;

    /// <summary>
    /// Masa limiti doldu mu?
    /// </summary>
    public bool IsTableLimitReached => TableCount >= TableLimit;
}
