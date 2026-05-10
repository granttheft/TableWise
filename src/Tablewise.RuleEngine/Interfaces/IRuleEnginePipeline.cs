using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Interfaces;

/// <summary>
/// Kural motoru ana pipeline interface'i.
/// Tüm kuralları sırayla değerlendirir ve sonuç döner.
/// </summary>
public interface IRuleEnginePipeline
{
    /// <summary>
    /// Pipeline'ı çalıştırır ve tüm kuralları değerlendirir.
    /// </summary>
    /// <param name="context">Rezervasyon context'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Toplam değerlendirme sonucu</returns>
    Task<PipelineResult> ExecuteAsync(
        ReservationContext context,
        CancellationToken cancellationToken = default);
}
