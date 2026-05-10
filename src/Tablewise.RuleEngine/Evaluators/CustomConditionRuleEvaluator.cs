using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Base;
using Tablewise.RuleEngine.Evaluators.Models;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;
using Tablewise.RuleEngine.Services;

namespace Tablewise.RuleEngine.Evaluators;

/// <summary>
/// Custom condition kuralı değerlendiricisi.
/// İşletmenin kendi koşul mantığını tanımlamasına izin verir.
/// Güvenlik kritik: eval, Expression.Compile, dynamic invoke KULLANMAZ.
/// </summary>
public sealed class CustomConditionRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public CustomConditionRuleEvaluator(ILogger<CustomConditionRuleEvaluator> logger)
        : base(logger)
    {
    }

    /// <inheritdoc />
    public override string RuleType => "custom_condition";

    /// <inheritdoc />
    public override Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Koşulları parse et
        var conditions = ParseConditions<CustomConditionConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Aksiyonları parse et
        var actions = ParseActions<CustomConditionActions>(rule, ExpectedVersion);
        if (actions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Koşul yoksa kural tetiklenmez
        if (conditions.Conditions == null || conditions.Conditions.Length == 0)
        {
            Logger.LogDebug(
                "Custom condition kuralı atlandı. Koşul tanımlı değil. RuleId={RuleId}",
                rule.Id);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Koşulları değerlendir
        var (satisfied, evaluations) = EvaluateConditions(conditions, context);

        if (!satisfied)
        {
            Logger.LogDebug(
                "Custom condition kuralı tetiklenmedi. RuleId={RuleId}, Operator={Operator}",
                rule.Id, conditions.Operator);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Aksiyon tipini belirle
        var (actionType, payload) = DetermineAction(actions);

        var message = !string.IsNullOrEmpty(actions.Message)
            ? actions.Message
            : "Custom kural tetiklendi";

        Logger.LogInformation(
            "Custom condition kuralı tetiklendi. RuleId={RuleId}, ActionType={ActionType}",
            rule.Id, actionType);

        var outcome = CreateOutcome(rule, actionType, message, payload);
        return Task.FromResult<RuleOutcome?>(outcome);
    }

    /// <summary>
    /// Tüm koşulları değerlendirir.
    /// </summary>
    /// <param name="conditions">Koşul tanımları</param>
    /// <param name="context">Rezervasyon context'i</param>
    /// <returns>Sonuç ve değerlendirme detayları</returns>
    public (bool Satisfied, List<ConditionEvaluation> Evaluations) EvaluateConditions(
        CustomConditionConditions conditions,
        ReservationContext context)
    {
        var evaluations = new List<ConditionEvaluation>();
        var isAnd = conditions.Operator.Equals("and", StringComparison.OrdinalIgnoreCase);

        foreach (var condition in conditions.Conditions)
        {
            var result = EvaluateSingleCondition(condition, context);
            evaluations.Add(result);

            if (result.Result)
            {
                // OR operatörü: tek true yeterli
                if (!isAnd)
                {
                    return (true, evaluations);
                }
            }
            else
            {
                // AND operatörü: tek false yeterli
                if (isAnd)
                {
                    return (false, evaluations);
                }
            }
        }

        // AND: tümü true ise true, OR: hiçbiri true değilse false
        return (isAnd, evaluations);
    }

    /// <summary>
    /// Tek bir koşulu değerlendirir.
    /// </summary>
    private ConditionEvaluation EvaluateSingleCondition(CustomCondition condition, ReservationContext context)
    {
        var evaluation = new ConditionEvaluation
        {
            Field = condition.Field,
            Op = condition.Op,
            ExpectedValue = condition.Value
        };

        // Field değerini al
        var fieldValue = FieldResolver.GetFieldValue(context, condition.Field, Logger);
        evaluation.ActualValue = fieldValue;

        // Null kontrolü
        if (fieldValue == null)
        {
            // Field değeri null ise koşul false
            evaluation.Result = false;
            return evaluation;
        }

        // Operatöre göre karşılaştır
        evaluation.Result = CompareValues(fieldValue, condition.Op, condition.Value);
        return evaluation;
    }

    /// <summary>
    /// İki değeri belirtilen operatöre göre karşılaştırır.
    /// </summary>
    private bool CompareValues(object? actualValue, string op, object? expectedValue)
    {
        if (actualValue == null)
            return false;

        try
        {
            return op.ToLowerInvariant() switch
            {
                "==" => AreEqual(actualValue, expectedValue),
                "!=" => !AreEqual(actualValue, expectedValue),
                "<" => CompareLessThan(actualValue, expectedValue),
                "<=" => CompareLessThanOrEqual(actualValue, expectedValue),
                ">" => CompareGreaterThan(actualValue, expectedValue),
                ">=" => CompareGreaterThanOrEqual(actualValue, expectedValue),
                "in" => IsIn(actualValue, expectedValue),
                "contains" => Contains(actualValue, expectedValue),
                _ => HandleUnknownOperator(op)
            };
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex,
                "Karşılaştırma hatası. ActualValue={ActualValue}, Op={Op}, ExpectedValue={ExpectedValue}",
                actualValue, op, expectedValue);
            return false;
        }
    }

    /// <summary>
    /// Eşitlik kontrolü.
    /// </summary>
    private static bool AreEqual(object? actual, object? expected)
    {
        if (actual == null && expected == null) return true;
        if (actual == null || expected == null) return false;

        // String karşılaştırması (case-insensitive)
        if (actual is string actualStr && expected is string expectedStr)
            return actualStr.Equals(expectedStr, StringComparison.OrdinalIgnoreCase);

        // Sayısal karşılaştırma
        if (TryConvertToDouble(actual, out var actualNum) && TryConvertToDouble(expected, out var expectedNum))
            return Math.Abs(actualNum - expectedNum) < 0.0001;

        return actual.ToString()?.Equals(expected.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Küçüktür kontrolü.
    /// </summary>
    private static bool CompareLessThan(object actual, object? expected)
    {
        if (!TryConvertToDouble(actual, out var actualNum) || !TryConvertToDouble(expected, out var expectedNum))
            return false;

        return actualNum < expectedNum;
    }

    /// <summary>
    /// Küçük eşittir kontrolü.
    /// </summary>
    private static bool CompareLessThanOrEqual(object actual, object? expected)
    {
        if (!TryConvertToDouble(actual, out var actualNum) || !TryConvertToDouble(expected, out var expectedNum))
            return false;

        return actualNum <= expectedNum;
    }

    /// <summary>
    /// Büyüktür kontrolü.
    /// </summary>
    private static bool CompareGreaterThan(object actual, object? expected)
    {
        if (!TryConvertToDouble(actual, out var actualNum) || !TryConvertToDouble(expected, out var expectedNum))
            return false;

        return actualNum > expectedNum;
    }

    /// <summary>
    /// Büyük eşittir kontrolü.
    /// </summary>
    private static bool CompareGreaterThanOrEqual(object actual, object? expected)
    {
        if (!TryConvertToDouble(actual, out var actualNum) || !TryConvertToDouble(expected, out var expectedNum))
            return false;

        return actualNum >= expectedNum;
    }

    /// <summary>
    /// "in" operatörü - değer listede mi?
    /// </summary>
    private static bool IsIn(object actual, object? expected)
    {
        if (expected == null) return false;

        var actualStr = actual.ToString() ?? string.Empty;

        // JsonElement dizisi kontrolü
        if (expected is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in jsonElement.EnumerateArray())
            {
                var itemStr = item.GetString() ?? item.ToString();
                if (actualStr.Equals(itemStr, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // String dizisi kontrolü
        if (expected is string[] stringArray)
        {
            return stringArray.Any(s => actualStr.Equals(s, StringComparison.OrdinalIgnoreCase));
        }

        // Object dizisi kontrolü
        if (expected is object[] objArray)
        {
            return objArray.Any(o => actualStr.Equals(o?.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        // IEnumerable kontrolü
        if (expected is System.Collections.IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
            {
                if (actualStr.Equals(item?.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        return false;
    }

    /// <summary>
    /// "contains" operatörü - string içinde arama.
    /// </summary>
    private static bool Contains(object actual, object? expected)
    {
        if (expected == null) return false;

        var actualStr = actual.ToString() ?? string.Empty;
        var expectedStr = expected.ToString() ?? string.Empty;

        return actualStr.Contains(expectedStr, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Bilinmeyen operatör için false döner.
    /// </summary>
    private bool HandleUnknownOperator(string op)
    {
        Logger.LogWarning("Bilinmeyen operatör: {Operator}", op);
        return false;
    }

    /// <summary>
    /// Object'i double'a çevirmeyi dener.
    /// </summary>
    private static bool TryConvertToDouble(object? value, out double result)
    {
        result = 0;
        if (value == null) return false;

        // JsonElement kontrolü
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.TryGetDouble(out result),
                JsonValueKind.String => double.TryParse(jsonElement.GetString(), out result),
                _ => false
            };
        }

        // Doğrudan sayısal tipler
        if (value is int intVal) { result = intVal; return true; }
        if (value is long longVal) { result = longVal; return true; }
        if (value is float floatVal) { result = floatVal; return true; }
        if (value is double doubleVal) { result = doubleVal; return true; }
        if (value is decimal decimalVal) { result = (double)decimalVal; return true; }

        // String'den dönüştürme
        return double.TryParse(value.ToString(), out result);
    }

    /// <summary>
    /// Aksiyon tipini ve payload'ı belirler.
    /// </summary>
    private static (RuleActionType ActionType, string? Payload) DetermineAction(CustomConditionActions actions)
    {
        if (actions.Block)
        {
            return (RuleActionType.Block, null);
        }

        if (actions.Warn)
        {
            return (RuleActionType.Warn, null);
        }

        if (actions.DiscountPercent.HasValue && actions.DiscountPercent.Value > 0)
        {
            var payload = JsonSerializer.Serialize(new { discountPercent = actions.DiscountPercent.Value });
            return (RuleActionType.Discount, payload);
        }

        if (actions.RequireDeposit)
        {
            var payload = JsonSerializer.Serialize(new { depositAmount = actions.DepositAmount ?? 0m });
            return (RuleActionType.Deposit, payload);
        }

        if (actions.Suggest)
        {
            return (RuleActionType.Suggest, null);
        }

        return (RuleActionType.Allow, null);
    }
}

/// <summary>
/// Tek bir koşulun değerlendirme sonucu.
/// Debug ve test için kullanılır.
/// </summary>
public sealed class ConditionEvaluation
{
    /// <summary>
    /// Alan adı.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Operatör.
    /// </summary>
    public string Op { get; set; } = string.Empty;

    /// <summary>
    /// Beklenen değer.
    /// </summary>
    public object? ExpectedValue { get; set; }

    /// <summary>
    /// Gerçek değer.
    /// </summary>
    public object? ActualValue { get; set; }

    /// <summary>
    /// Koşul sonucu (true/false).
    /// </summary>
    public bool Result { get; set; }
}
