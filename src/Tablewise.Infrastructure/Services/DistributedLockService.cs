using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Tablewise.Application.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Redis tabanlı dağıtık kilit servisi.
/// Race condition'ları önlemek için SETNX + TTL kullanır.
/// </summary>
public sealed class DistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<DistributedLockService> _logger;

    private const string LockKeyPrefix = "lock:";
    private const int RetryDelayMs = 50;
    private const int MaxRetryDelayMs = 500;

    /// <summary>
    /// DistributedLockService constructor.
    /// </summary>
    public DistributedLockService(
        IConnectionMultiplexer redis,
        ILogger<DistributedLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string lockKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var fullKey = $"{LockKeyPrefix}{lockKey}";
        var lockValue = Guid.NewGuid().ToString();

        try
        {
            var db = _redis.GetDatabase();
            var acquired = await db.StringSetAsync(
                fullKey,
                lockValue,
                expiry,
                When.NotExists)
                .ConfigureAwait(false);

            if (acquired)
            {
                _logger.LogDebug("Distributed lock alındı: {LockKey}", lockKey);
                return new RedisLockHandle(_redis, fullKey, lockValue, _logger);
            }

            _logger.LogDebug("Distributed lock alınamadı (meşgul): {LockKey}", lockKey);
            return null;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası, lock alınamadı: {LockKey}", lockKey);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IDistributedLockHandle?> WaitForLockAsync(
        string lockKey,
        TimeSpan expiry,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var currentDelay = RetryDelayMs;

        while (DateTime.UtcNow - startTime < waitTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handle = await TryAcquireAsync(lockKey, expiry, cancellationToken).ConfigureAwait(false);
            if (handle != null)
            {
                return handle;
            }

            await Task.Delay(currentDelay, cancellationToken).ConfigureAwait(false);

            // Exponential backoff
            currentDelay = Math.Min(currentDelay * 2, MaxRetryDelayMs);
        }

        _logger.LogWarning("Lock timeout: {LockKey}, WaitTimeout: {Timeout}ms", lockKey, waitTimeout.TotalMilliseconds);
        return null;
    }
}

/// <summary>
/// Redis lock handle. Dispose edildiğinde lock serbest bırakılır.
/// </summary>
internal sealed class RedisLockHandle : IDistributedLockHandle
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _fullKey;
    private readonly string _lockValue;
    private readonly ILogger _logger;
    private bool _released;

    /// <summary>
    /// RedisLockHandle constructor.
    /// </summary>
    public RedisLockHandle(
        IConnectionMultiplexer redis,
        string fullKey,
        string lockValue,
        ILogger logger)
    {
        _redis = redis;
        _fullKey = fullKey;
        _lockValue = lockValue;
        _logger = logger;
        _released = false;
    }

    /// <inheritdoc />
    public bool IsAcquired => !_released;

    /// <inheritdoc />
    public string LockKey => _fullKey;

    /// <inheritdoc />
    public async Task ReleaseAsync()
    {
        if (_released)
            return;

        try
        {
            var db = _redis.GetDatabase();

            // Lua script ile atomic olarak sadece kendi lock'umuzu sil
            const string script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            await db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { _fullKey },
                new RedisValue[] { _lockValue })
                .ConfigureAwait(false);

            _released = true;
            _logger.LogDebug("Distributed lock serbest bırakıldı: {LockKey}", _fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lock release başarısız: {LockKey}", _fullKey);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync().ConfigureAwait(false);
    }
}
