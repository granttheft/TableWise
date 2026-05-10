using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Interfaces;

/// <summary>
/// Kural test servisi interface.
/// Kuralları test etmek için kullanılır.
/// </summary>
public interface IRuleTestService
{
    /// <summary>
    /// Belirtilen kuralı test eder.
    /// </summary>
    /// <param name="ruleId">Kural ID</param>
    /// <param name="request">Test parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kural değerlendirme sonucu</returns>
    Task<RuleEvaluationResult> TestRuleAsync(
        Guid ruleId,
        TestRuleRequestDto request,
        CancellationToken cancellationToken = default);
}
