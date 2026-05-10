using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Base;
using Tablewise.RuleEngine.Evaluators.Models;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Evaluators;

/// <summary>
/// Erken rezervasyon indirim kuralı değerlendiricisi.
/// Belirli gün öncesinden yapılan rezervasyonlara indirim uygular.
/// </summary>
public sealed class EarlyBookingRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public EarlyBookingRuleEvaluator(ILogger<EarlyBookingRuleEvaluator> logger)
        : base(logger)
    {
    }

    /// <inheritdoc />
    public override string RuleType => "early_booking";

    /// <inheritdoc />
    public override Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Koşulları parse et
        var conditions = ParseConditions<EarlyBookingConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Aksiyonları parse et
        var actions = ParseActions<EarlyBookingActions>(rule, ExpectedVersion);
        if (actions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Minimum gün kontrolü
        if (context.DaysInAdvance < conditions.MinDaysInAdvance)
        {
            Logger.LogDebug(
                "Erken rezervasyon kuralı tetiklenmedi. DaysInAdvance={DaysInAdvance} < Min={Min}",
                context.DaysInAdvance, conditions.MinDaysInAdvance);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Maksimum gün kontrolü (opsiyonel)
        if (conditions.MaxDaysInAdvance.HasValue &&
            context.DaysInAdvance > conditions.MaxDaysInAdvance.Value)
        {
            Logger.LogDebug(
                "Erken rezervasyon kuralı tetiklenmedi. DaysInAdvance={DaysInAdvance} > Max={Max}",
                context.DaysInAdvance, conditions.MaxDaysInAdvance.Value);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // İndirim uygula
        var discountPercent = actions.DiscountPercent ?? 0m;
        var message = actions.Label ?? "Erken rezervasyon indirimi uygulandı";

        var payload = JsonSerializer.Serialize(new { discountPercent });

        Logger.LogInformation(
            "Erken rezervasyon indirimi uygulandı. RuleId={RuleId}, Discount={Discount}%",
            rule.Id, discountPercent);

        var outcome = CreateOutcome(rule, RuleActionType.Discount, message, payload);
        return Task.FromResult<RuleOutcome?>(outcome);
    }
}
