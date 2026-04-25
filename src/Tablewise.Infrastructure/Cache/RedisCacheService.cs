using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tablewise.Application.Interfaces;

namespace Tablewise.Infrastructure.Cache;

/// <summary>
/// Redis cache servisi implementation.
/// </summary>
public sealed class RedisCacheService : ICacheService, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    /// <summary>
    /// RedisCacheService constructor.
    /// </summary>
    public RedisCacheService(
        IConnectionMultiplexer redis,
        IOptions<RedisSettings> settings,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _settings = settings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        try
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            await _db.StringSetAsync(fullKey, serialized, expiry).ConfigureAwait(false);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası. Key: {Key}", fullKey);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        try
        {
            var value = await _db.StringGetAsync(fullKey).ConfigureAwait(false);

            if (!value.HasValue)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası. Key: {Key}", fullKey);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "JSON deserialize hatası. Key: {Key}", fullKey);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        try
        {
            return await _db.KeyDeleteAsync(fullKey).ConfigureAwait(false);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası. Key: {Key}", fullKey);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        try
        {
            return await _db.KeyExistsAsync(fullKey).ConfigureAwait(false);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası. Key: {Key}", fullKey);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<long> IncrementAsync(
        string key,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        try
        {
            var newValue = await _db.StringIncrementAsync(fullKey).ConfigureAwait(false);

            // İlk increment ise expiry ayarla
            if (newValue == 1 && expiry.HasValue)
            {
                await _db.KeyExpireAsync(fullKey, expiry.Value).ConfigureAwait(false);
            }

            return newValue;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası. Key: {Key}", fullKey);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TimeSpan?> GetTimeToLiveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var fullKey = GetFullKey(key);

        try
        {
            return await _db.KeyTimeToLiveAsync(fullKey).ConfigureAwait(false);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası. Key: {Key}", fullKey);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(
        string pattern,
        CancellationToken cancellationToken = default)
    {
        var fullPattern = GetFullKey(pattern);

        try
        {
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: fullPattern).ToArray();

                if (keys.Length > 0)
                {
                    await _db.KeyDeleteAsync(keys).ConfigureAwait(false);
                }
            }
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis bağlantı hatası. Pattern: {Pattern}", fullPattern);
            throw;
        }
    }

    /// <summary>
    /// Key prefix ekler.
    /// </summary>
    private string GetFullKey(string key)
    {
        return $"{_settings.KeyPrefix}{key}";
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
