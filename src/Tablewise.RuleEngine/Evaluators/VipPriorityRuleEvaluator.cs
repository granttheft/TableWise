using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Base;
using Tablewise.RuleEngine.Evaluators.Models;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Evaluators;

/// <summary>
/// VIP müşteri öncelik kuralı değerlendiricisi.
/// VIP/Gold müşterilere en iyi masayı önerir.
/// </summary>
public sealed class VipPriorityRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public VipPriorityRuleEvaluator(ILogger<VipPriorityRuleEvaluator> logger)
        : base(logger)
    {
    }

    /// <inheritdoc />
    public override string RuleType => "vip_priority";

    /// <inheritdoc />
    public override Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Müşteri yoksa (kayıtsız misafir) kural uygulanamaz
        if (context.Customer == null)
        {
            Logger.LogDebug(
                "VIP öncelik kuralı atlandı. Kayıtlı müşteri yok.");
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Koşulları parse et
        var conditions = ParseConditions<VipPriorityConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Aksiyonları parse et
        var actions = ParseActions<VipPriorityActions>(rule, ExpectedVersion);
        if (actions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Minimum tier'ı belirle
        var minTier = ParseTier(conditions.MinTier);

        // Müşteri tier'ı kontrol et (Blacklisted hariç)
        var customerTier = context.Customer.Tier;
        if (customerTier == CustomerTier.Blacklisted)
        {
            Logger.LogDebug(
                "VIP öncelik kuralı atlandı. Müşteri kara listede.");
            return Task.FromResult<RuleOutcome?>(null);
        }

        if (customerTier < minTier)
        {
            Logger.LogDebug(
                "VIP öncelik kuralı atlandı. CustomerTier={Tier} < MinTier={MinTier}",
                customerTier, minTier);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // VIP müşteri - öneri oluştur
        var message = "VIP misafiriniz için özel masa ayrıldı";

        Logger.LogInformation(
            "VIP öncelik kuralı tetiklendi. RuleId={RuleId}, CustomerTier={Tier}",
            rule.Id, customerTier);

        var outcome = CreateOutcome(rule, RuleActionType.Suggest, message);
        return Task.FromResult<RuleOutcome?>(outcome);
    }

    /// <summary>
    /// String tier değerini enum'a çevirir.
    /// </summary>
    private static CustomerTier ParseTier(string tierString)
    {
        return tierString?.ToLowerInvariant() switch
        {
            "gold" => CustomerTier.Gold,
            "vip" => CustomerTier.VIP,
            _ => CustomerTier.Gold // Varsayılan Gold
        };
    }
}
