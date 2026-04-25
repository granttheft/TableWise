namespace Tablewise.Application.Interfaces;

/// <summary>
/// Cache servisi interface (Redis implementation).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Değer kaydeder.
    /// </summary>
    /// <typeparam name="T">Değer tipi</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Değer</param>
    /// <param name="expiry">Son kullanma süresi (nullable - null ise sonsuz)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Değer okur.
    /// </summary>
    /// <typeparam name="T">Değer tipi</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Değer veya default</returns>
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Değer siler.
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silindi ise true</returns>
    Task<bool> RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Key var mı kontrol eder.
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Var ise true</returns>
    Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sayaç artırır (atomic increment).
    /// Key yoksa 1'den başlatır.
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="expiry">Son kullanma süresi (ilk oluşturmada)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yeni değer</returns>
    Task<long> IncrementAsync(
        string key,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Key'in kalan süresini alır.
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kalan süre veya null (sonsuz veya yok)</returns>
    Task<TimeSpan?> GetTimeToLiveAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pattern'e uyan key'leri siler.
    /// </summary>
    /// <param name="pattern">Key pattern (örn: "login_fail:*")</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task RemoveByPatternAsync(
        string pattern,
        CancellationToken cancellationToken = default);
}
