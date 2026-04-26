using Tablewise.Application.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Stub kural değerlendiricisi. Faz 3'te gerçek implementasyon ile değiştirilecek.
/// Şimdilik tüm istekleri onaylıyor.
/// </summary>
public sealed class StubRuleEvaluator : IRuleEvaluator
{
    /// <inheritdoc />
    public Task<RuleEvaluationResult> EvaluateAsync(
        RuleEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        // Faz 3'te gerçek kural motoru implemente edilecek
        // Şimdilik tüm istekleri onaylıyoruz
        return Task.FromResult(RuleEvaluationResult.Allow());
    }
}
