namespace Tablewise.Infrastructure.Cache;

/// <summary>
/// Redis yapılandırma ayarları.
/// </summary>
public sealed class RedisSettings
{
    /// <summary>
    /// Configuration section adı.
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis connection string.
    /// Format: "localhost:6379,password=xxx,ssl=false"
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Key prefix (multi-tenant isolation için).
    /// </summary>
    public string KeyPrefix { get; set; } = "tablewise:";

    /// <summary>
    /// Default cache süresi (saniye). 0 = sonsuz.
    /// </summary>
    public int DefaultExpirationSeconds { get; set; } = 3600;

    /// <summary>
    /// Connection retry sayısı.
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// Connection timeout (ms).
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;
}
