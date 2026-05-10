using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Tablewise.Infrastructure.Email.Models;

namespace Tablewise.Infrastructure.Email.Services;

/// <summary>
/// Email'leri Redis queue'ya yazar. Background worker tarafından tüketilir.
/// </summary>
public sealed class EmailQueueService
{
    private const string QueueKey = "tablewise:email:queue";
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<EmailQueueService> _logger;

    public EmailQueueService(IConnectionMultiplexer redis, ILogger<EmailQueueService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Email request'i queue'ya ekler (LPUSH).
    /// </summary>
    public async Task EnqueueAsync(EmailRequest request, CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(request);
            await db.ListLeftPushAsync(QueueKey, json).ConfigureAwait(false);

            _logger.LogInformation("Email queue'ya eklendi. To={To}, Template={Template}", 
                request.To, request.TemplateName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email queue'ya eklenemedi. To={To}", request.To);
            throw;
        }
    }

    /// <summary>
    /// Queue'dan email alır (BRPOP blocking). Worker tarafından kullanılır.
    /// </summary>
    public async Task<EmailRequest?> DequeueAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var result = await db.ListRightPopAsync(QueueKey).ConfigureAwait(false);

            if (result.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<EmailRequest>(result.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email queue'dan alınamadı");
            return null;
        }
    }
}
