using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Idempotency servisi. Redis + DB fallback ile duplicate request'leri önler.
/// </summary>
public sealed class IdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly TablewiseDbContext _dbContext;
    private readonly ILogger<IdempotencyService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private const int RedisTtlSeconds = 60;
    private const int DbTtlHours = 24;
    private const string RedisKeyPrefix = "idem:";

    /// <summary>
    /// IdempotencyService constructor.
    /// </summary>
    public IdempotencyService(
        IConnectionMultiplexer redis,
        TablewiseDbContext dbContext,
        ILogger<IdempotencyService> logger)
    {
        _redis = redis;
        _dbContext = dbContext;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public async Task<CachedIdempotencyResponse?> GetAsync(
        Guid tenantId,
        string key,
        CancellationToken cancellationToken = default)
    {
        var redisKey = BuildRedisKey(tenantId, key);

        // Önce Redis'ten dene
        try
        {
            if (_redis.IsConnected)
            {
                var db = _redis.GetDatabase();
                var cached = await db.StringGetAsync(redisKey).ConfigureAwait(false);

                if (cached.HasValue)
                {
                    _logger.LogDebug("Idempotency key Redis'te bulundu: {Key}", key);
                    return JsonSerializer.Deserialize<CachedIdempotencyResponse>(cached.ToString(), _jsonOptions);
                }
            }
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis bağlantı hatası, DB fallback kullanılıyor. Key: {Key}", key);
        }

        // Redis'te yoksa veya Redis down ise DB'den kontrol et
        var dbRecord = await _dbContext.IdempotencyKeys
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.Key == key && !i.IsDeleted && i.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (dbRecord != null)
        {
            _logger.LogDebug("Idempotency key DB'de bulundu: {Key}", key);
            return JsonSerializer.Deserialize<CachedIdempotencyResponse>(dbRecord.ResponseJson, _jsonOptions);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task SaveAsync(
        Guid tenantId,
        string key,
        CachedIdempotencyResponse response,
        CancellationToken cancellationToken = default)
    {
        var redisKey = BuildRedisKey(tenantId, key);
        var json = JsonSerializer.Serialize(response, _jsonOptions);

        // Redis'e yaz (60 sn TTL)
        try
        {
            if (_redis.IsConnected)
            {
                var db = _redis.GetDatabase();
                await db.StringSetAsync(
                    redisKey,
                    json,
                    TimeSpan.FromSeconds(RedisTtlSeconds))
                    .ConfigureAwait(false);

                _logger.LogDebug("Idempotency key Redis'e yazıldı: {Key}, TTL: {Ttl}s", key, RedisTtlSeconds);
            }
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis'e yazılamadı, sadece DB'ye yazılacak. Key: {Key}", key);
        }

        // DB'ye yaz (24 saat TTL)
        var dbRecord = new IdempotencyKey
        {
            TenantId = tenantId,
            Key = key,
            ResponseJson = json,
            ExpiresAt = DateTime.UtcNow.AddHours(DbTtlHours)
        };

        _dbContext.IdempotencyKeys.Add(dbRecord);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Idempotency key DB'ye yazıldı: {Key}, TTL: {Ttl}h", key, DbTtlHours);
    }

    /// <inheritdoc />
    public async Task<bool> IsRedisAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_redis.IsConnected)
                return false;

            var db = _redis.GetDatabase();
            await db.PingAsync().ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Redis key oluşturur.
    /// </summary>
    private static string BuildRedisKey(Guid tenantId, string key)
    {
        return $"{RedisKeyPrefix}{tenantId}:{key}";
    }
}
