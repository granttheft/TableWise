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
    /// <returns>Kural test sonucu</returns>
    Task<RuleTestResultDto> TestRuleAsync(
        Guid ruleId,
        TestRuleRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kaydedilmemiş kural tanımını test eder (taslak JSON ile).
    /// </summary>
    /// <param name="request">Taslak kural ve test bağlamı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Test sonucu</returns>
    Task<RuleTestResultDto> TestDraftRuleAsync(
        TestDraftRuleRequestDto request,
        CancellationToken cancellationToken = default);
}
