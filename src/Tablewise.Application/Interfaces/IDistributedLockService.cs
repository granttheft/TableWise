namespace Tablewise.Application.Interfaces;

/// <summary>
/// Dağıtık kilit servisi.
/// Race condition'ları önlemek için Redis veya PostgreSQL advisory lock kullanır.
/// </summary>
public interface IDistributedLockService
{
    /// <summary>
    /// Kilit almaya çalışır.
    /// </summary>
    /// <param name="lockKey">Kilit anahtarı</param>
    /// <param name="expiry">Kilit süresi (auto-release)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kilit alındıysa handle, alınamadıysa null</returns>
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kilit alana kadar bekler (timeout ile).
    /// </summary>
    /// <param name="lockKey">Kilit anahtarı</param>
    /// <param name="expiry">Kilit süresi (auto-release)</param>
    /// <param name="waitTimeout">Bekleme timeout'u</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kilit alındıysa handle, timeout olduysa null</returns>
    Task<IDistributedLockHandle?> WaitForLockAsync(
        string lockKey,
        TimeSpan expiry,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Dağıtık kilit handle'ı.
/// Dispose edildiğinde kilit serbest bırakılır.
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
    /// <summary>
    /// Kilit hala geçerli mi?
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// Kilit anahtarı.
    /// </summary>
    string LockKey { get; }

    /// <summary>
    /// Kilidi manuel olarak serbest bırakır.
    /// </summary>
    Task ReleaseAsync();
}
