namespace Tablewise.RuleEngine.Interfaces;

/// <summary>
/// Kural tipine göre uygun evaluator'ı döndüren factory.
/// </summary>
public interface IRuleTypeEvaluatorFactory
{
    /// <summary>
    /// Belirtilen kural tipi için evaluator döndürür.
    /// </summary>
    /// <param name="ruleType">Kural tipi (Rule.RuleType)</param>
    /// <returns>
    /// Evaluator veya null.
    /// Null dönerse bu tip için evaluator tanımlanmamış demektir.
    /// Factory exception atmaz - bilinmeyen tipler için null döner.
    /// </returns>
    IRuleTypeEvaluator? GetFor(string ruleType);
}
