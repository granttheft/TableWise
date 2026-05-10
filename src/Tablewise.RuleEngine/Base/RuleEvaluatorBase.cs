using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Interfaces;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Base;

/// <summary>
/// Tüm kural evaluator'ları için abstract base class.
/// JSON parsing ve version kontrolü sağlar.
/// </summary>
public abstract class RuleEvaluatorBase : IRuleTypeEvaluator
{
    /// <summary>
    /// Logger instance.
    /// </summary>
    protected readonly ILogger Logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Constructor.
    /// </summary>
    protected RuleEvaluatorBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <inheritdoc />
    public abstract string RuleType { get; }

    /// <inheritdoc />
    public abstract Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ConditionsJson'ı parse eder.
    /// </summary>
    /// <typeparam name="T">Conditions tipi</typeparam>
    /// <param name="rule">Kural</param>
    /// <param name="expectedVersion">Beklenen version (0 ise kontrol yok)</param>
    /// <returns>Parse edilmiş conditions veya null</returns>
    protected T? ParseConditions<T>(Rule rule, int expectedVersion = 0) where T : class, IVersionedJson
    {
        return ParseJson<T>(rule.ConditionsJson, "Conditions", rule, expectedVersion);
    }

    /// <summary>
    /// ActionsJson'ı parse eder.
    /// </summary>
    /// <typeparam name="T">Actions tipi</typeparam>
    /// <param name="rule">Kural</param>
    /// <param name="expectedVersion">Beklenen version (0 ise kontrol yok)</param>
    /// <returns>Parse edilmiş actions veya null</returns>
    protected T? ParseActions<T>(Rule rule, int expectedVersion = 0) where T : class, IVersionedJson
    {
        return ParseJson<T>(rule.ActionsJson, "Actions", rule, expectedVersion);
    }

    /// <summary>
    /// JSON string'i parse eder ve version kontrolü yapar.
    /// </summary>
    private T? ParseJson<T>(string json, string fieldName, Rule rule, int expectedVersion) where T : class, IVersionedJson
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Logger.LogWarning(
                "Kural {RuleId} ({RuleName}) için {FieldName} boş veya null",
                rule.Id, rule.Name, fieldName);
            return null;
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);

            if (result == null)
            {
                Logger.LogWarning(
                    "Kural {RuleId} ({RuleName}) için {FieldName} parse edilemedi",
                    rule.Id, rule.Name, fieldName);
                return null;
            }

            // Version kontrolü
            if (expectedVersion > 0 && result.Version != expectedVersion)
            {
                Logger.LogWarning(
                    "Kural {RuleId} ({RuleName}) için {FieldName} version uyumsuz. " +
                    "Beklenen: {ExpectedVersion}, Bulunan: {FoundVersion}. " +
                    "Kural yine de çalıştırılacak.",
                    rule.Id, rule.Name, fieldName, expectedVersion, result.Version);
                // Uyumsuz version'da devam et, Sentry'ye loglandı
            }

            return result;
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex,
                "Kural {RuleId} ({RuleName}) için {FieldName} JSON parse hatası: {Json}",
                rule.Id, rule.Name, fieldName, json);
            return null;
        }
    }

    /// <summary>
    /// Başarılı outcome oluşturur.
    /// </summary>
    protected static RuleOutcome CreateOutcome(
        Rule rule,
        Domain.Enums.RuleActionType actionType,
        string? message = null,
        string? payload = null)
    {
        return new RuleOutcome
        {
            RuleId = rule.Id,
            RuleName = rule.Name,
            ActionType = actionType,
            Message = message,
            Payload = payload,
            Priority = rule.Priority,
            EvaluatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Versioned JSON için interface.
/// Tüm conditions/actions sınıfları bunu implement etmeli.
/// </summary>
public interface IVersionedJson
{
    /// <summary>
    /// Şema versiyonu. Migration için kullanılır.
    /// </summary>
    int Version { get; }
}
