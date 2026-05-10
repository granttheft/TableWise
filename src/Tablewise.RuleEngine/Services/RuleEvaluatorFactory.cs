using Tablewise.RuleEngine.Interfaces;

namespace Tablewise.RuleEngine.Services;

/// <summary>
/// Kural tipine göre evaluator döndüren factory implementation.
/// DI container'dan inject edilen evaluator'ları dictionary'de tutar.
/// </summary>
public sealed class RuleTypeEvaluatorFactory : IRuleTypeEvaluatorFactory
{
    private readonly Dictionary<string, IRuleTypeEvaluator> _evaluators;

    /// <summary>
    /// Constructor. Tüm evaluator'ları DI'dan alır ve dictionary'e map eder.
    /// </summary>
    /// <param name="evaluators">DI'dan inject edilen tüm evaluator'lar</param>
    public RuleTypeEvaluatorFactory(IEnumerable<IRuleTypeEvaluator> evaluators)
    {
        // RuleType'a göre dictionary oluştur
        // Aynı tip için birden fazla evaluator varsa son kaydedilen kazanır
        _evaluators = evaluators
            .GroupBy(e => e.RuleType, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Last(),
                StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public IRuleTypeEvaluator? GetFor(string ruleType)
    {
        if (string.IsNullOrWhiteSpace(ruleType))
            return null;

        _evaluators.TryGetValue(ruleType, out var evaluator);
        return evaluator;
    }
}
