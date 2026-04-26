namespace Tablewise.Application.Interfaces;

/// <summary>
/// Plan limit kontrol servisi.
/// Tenant'ın aktif planına göre limit kontrolü yapar.
/// </summary>
public interface IPlanLimitService
{
    /// <summary>
    /// Mekan limiti kontrolü yapar.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    /// <exception cref="Domain.Exceptions.PlanLimitExceededException">Limit aşıldı</exception>
    Task CheckVenueLimitAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Masa limiti kontrolü yapar.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="venueId">Venue ID (opsiyonel, venue bazlı limit için)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    /// <exception cref="Domain.Exceptions.PlanLimitExceededException">Limit aşıldı</exception>
    Task CheckTableLimitAsync(Guid tenantId, Guid? venueId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kural limiti kontrolü yapar.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    /// <exception cref="Domain.Exceptions.PlanLimitExceededException">Limit aşıldı</exception>
    Task CheckRuleLimitAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rezervasyon limiti kontrolü yapar (aylık).
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Task</returns>
    /// <exception cref="Domain.Exceptions.PlanLimitExceededException">Limit aşıldı</exception>
    Task CheckReservationLimitAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tenant'ın mevcut kullanım istatistiklerini getirir.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanım bilgileri</returns>
    Task<PlanUsageSummaryDto> GetTenantUsageAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Plan tier'ına göre aylık rezervasyon limitini döner.
    /// </summary>
    int GetMonthlyReservationLimit(Domain.Enums.PlanTier tier);

    /// <summary>
    /// Plan tier'ına göre mekan limitini döner.
    /// </summary>
    int GetVenueLimit(Domain.Enums.PlanTier tier);

    /// <summary>
    /// Plan tier'ına göre kullanıcı limitini döner.
    /// </summary>
    int GetUserLimit(Domain.Enums.PlanTier tier);

    /// <summary>
    /// Plan tier'ına göre masa limitini döner.
    /// </summary>
    int GetTableLimit(Domain.Enums.PlanTier tier);
}

/// <summary>
/// Plan kullanım özeti DTO'su.
/// IPlanLimitService tarafından döndürülür.
/// </summary>
public sealed record PlanUsageSummaryDto
{
    /// <summary>
    /// Mekan sayısı / limit.
    /// </summary>
    public required UsageInfo Venues { get; init; }

    /// <summary>
    /// Masa sayısı / limit.
    /// </summary>
    public required UsageInfo Tables { get; init; }

    /// <summary>
    /// Kural sayısı / limit.
    /// </summary>
    public required UsageInfo Rules { get; init; }

    /// <summary>
    /// Bu ay rezervasyon sayısı / limit.
    /// </summary>
    public required UsageInfo ReservationsThisMonth { get; init; }
}

/// <summary>
/// Kullanım bilgisi (mevcut / maksimum).
/// </summary>
public sealed record UsageInfo
{
    /// <summary>
    /// Mevcut kullanım.
    /// </summary>
    public required int Current { get; init; }

    /// <summary>
    /// Maksimum limit (-1 = sınırsız).
    /// </summary>
    public required int Maximum { get; init; }

    /// <summary>
    /// Yüzde kullanım.
    /// </summary>
    public int Percentage => Maximum > 0 ? (Current * 100 / Maximum) : 0;

    /// <summary>
    /// Limit aşıldı mı?
    /// </summary>
    public bool IsExceeded => Maximum > 0 && Current >= Maximum;
}
