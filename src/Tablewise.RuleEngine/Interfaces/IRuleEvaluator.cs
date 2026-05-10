using Tablewise.Domain.Entities;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Interfaces;

/// <summary>
/// Tek bir kural tipini değerlendiren evaluator interface'i.
/// Her kural tipi (EarlyBooking, VIPPriority vb.) için ayrı implementation.
/// Application.IRuleEvaluator'dan farklı - bu tek kural tipi için.
/// </summary>
public interface IRuleTypeEvaluator
{
    /// <summary>
    /// Bu evaluator'ın desteklediği kural tipi.
    /// Rule.RuleType alanı ile eşleşir.
    /// </summary>
    string RuleType { get; }

    /// <summary>
    /// Kuralı değerlendirir.
    /// </summary>
    /// <param name="rule">Değerlendirilecek kural</param>
    /// <param name="context">Rezervasyon context'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>
    /// Kural outcome'u veya null.
    /// Null dönerse kural bu context için tetiklenmedi demektir.
    /// </returns>
    Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default);
}
