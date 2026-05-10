using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Base;
using Tablewise.RuleEngine.Evaluators.Models;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Evaluators;

/// <summary>
/// Grup kompozisyonu kuralı değerlendiricisi.
/// Grup yapısına (cinsiyet dağılımı, kompozisyon tipi) göre kural uygular.
/// </summary>
public sealed class GroupCompositionRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public GroupCompositionRuleEvaluator(ILogger<GroupCompositionRuleEvaluator> logger)
        : base(logger)
    {
    }

    /// <inheritdoc />
    public override string RuleType => "group_composition";

    /// <inheritdoc />
    public override Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Grup bilgisi yoksa kural uygulanamaz
        if (context.GroupComposition == null && context.MaleCount == null)
        {
            Logger.LogDebug(
                "Grup kompozisyonu kuralı atlandı. Grup bilgisi mevcut değil.");
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Koşulları parse et
        var conditions = ParseConditions<GroupCompositionConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Aksiyonları parse et
        var actions = ParseActions<GroupCompositionActions>(rule, ExpectedVersion);
        if (actions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Alt kuralları değerlendir
        var violations = EvaluateSubRules(conditions.Rules, context, conditions.Operator);

        if (!violations.hasViolation)
        {
            Logger.LogDebug(
                "Grup kompozisyonu kuralı: İhlal yok. RuleId={RuleId}",
                rule.Id);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // İhlal var - aksiyon uygula
        RuleActionType actionType;
        if (actions.Block)
        {
            actionType = RuleActionType.Block;
            Logger.LogWarning(
                "Grup kompozisyonu engeli uygulandı. RuleId={RuleId}, Reason={Reason}",
                rule.Id, violations.reason);
        }
        else if (actions.Warn)
        {
            actionType = RuleActionType.Warn;
            Logger.LogInformation(
                "Grup kompozisyonu uyarısı uygulandı. RuleId={RuleId}, Reason={Reason}",
                rule.Id, violations.reason);
        }
        else
        {
            // Ne block ne warn
            return Task.FromResult<RuleOutcome?>(null);
        }

        var message = !string.IsNullOrEmpty(actions.Message)
            ? actions.Message
            : violations.reason;

        var outcome = CreateOutcome(rule, actionType, message);
        return Task.FromResult<RuleOutcome?>(outcome);
    }

    /// <summary>
    /// Alt kuralları değerlendirir.
    /// </summary>
    private (bool hasViolation, string reason) EvaluateSubRules(
        CompositionRule[] rules,
        ReservationContext context,
        string @operator)
    {
        if (rules == null || rules.Length == 0)
            return (false, string.Empty);

        var isAnd = @operator.Equals("and", StringComparison.OrdinalIgnoreCase);
        var violations = new List<string>();

        foreach (var subRule in rules)
        {
            var (violated, reason) = EvaluateSingleRule(subRule, context);

            if (violated)
            {
                violations.Add(reason);

                // OR operatörü: tek ihlal yeterli
                if (!isAnd)
                {
                    return (true, reason);
                }
            }
            else if (isAnd)
            {
                // AND operatörü: tüm kurallar ihlal edilmeli
                // Bir kural ihlal edilmediyse toplam ihlal yok
                return (false, string.Empty);
            }
        }

        // AND operatörü: tüm kurallar ihlal edildiyse
        if (isAnd && violations.Count == rules.Length)
        {
            return (true, string.Join("; ", violations));
        }

        return (false, string.Empty);
    }

    /// <summary>
    /// Tek bir alt kuralı değerlendirir.
    /// </summary>
    private (bool violated, string reason) EvaluateSingleRule(
        CompositionRule subRule,
        ReservationContext context)
    {
        return subRule.Type?.ToLowerInvariant() switch
        {
            "composition" => EvaluateCompositionRule(subRule, context),
            "ratio" => EvaluateRatioRule(subRule, context),
            _ => (false, string.Empty)
        };
    }

    /// <summary>
    /// Kompozisyon tipine göre kural değerlendirir.
    /// </summary>
    private (bool violated, string reason) EvaluateCompositionRule(
        CompositionRule subRule,
        ReservationContext context)
    {
        var composition = context.GroupComposition;

        // Kompozisyon bilgisi yoksa değerlendirilemez
        if (string.IsNullOrEmpty(composition))
            return (false, string.Empty);

        // Engellenen kompozisyonlar kontrolü
        if (subRule.BlockedCompositions != null && subRule.BlockedCompositions.Length > 0)
        {
            if (subRule.BlockedCompositions.Contains(composition, StringComparer.OrdinalIgnoreCase))
            {
                return (true, $"'{composition}' grup tipi kabul edilmemektedir");
            }
        }

        // İzin verilen kompozisyonlar kontrolü
        if (subRule.AllowedCompositions != null && subRule.AllowedCompositions.Length > 0)
        {
            if (!subRule.AllowedCompositions.Contains(composition, StringComparer.OrdinalIgnoreCase))
            {
                return (true, $"'{composition}' grup tipi bu mekan için uygun değil");
            }
        }

        return (false, string.Empty);
    }

    /// <summary>
    /// Cinsiyet oranına göre kural değerlendirir.
    /// </summary>
    private (bool violated, string reason) EvaluateRatioRule(
        CompositionRule subRule,
        ReservationContext context)
    {
        // Minimum kişi sayısı kontrolü
        if (subRule.MinPartySize.HasValue)
        {
            if (context.Reservation.PartySize < subRule.MinPartySize.Value)
            {
                // Kişi sayısı limiti altında, kural uygulanmaz
                return (false, string.Empty);
            }

            // Eğer sadece minPartySize varsa (oran kontrolü yoksa) ve
            // kişi sayısı limiti karşılanıyorsa, bu kural "sağlanmış" sayılır
            // (AND operatörüyle kullanıldığında diğer kurallarla birlikte değerlendirilir)
            var hasRatioCheck = subRule.MinFemaleRatio.HasValue || subRule.MaxMaleRatio.HasValue;
            if (!hasRatioCheck)
            {
                // Sadece minPartySize kontrolü var ve sağlandı - "ihlal edildi" say
                // (Bu, AND operatöründe kişi sayısı filtresi olarak çalışır)
                return (true, $"Kişi sayısı {subRule.MinPartySize.Value} veya üzeri");
            }
        }

        // Kadın oranı kontrolü
        if (subRule.MinFemaleRatio.HasValue)
        {
            // Veri yoksa değerlendirilemez
            if (!context.FemaleRatio.HasValue)
                return (false, string.Empty);

            if (context.FemaleRatio.Value < subRule.MinFemaleRatio.Value)
            {
                var requiredPercent = (int)(subRule.MinFemaleRatio.Value * 100);
                var currentPercent = (int)(context.FemaleRatio.Value * 100);
                return (true, $"Minimum %{requiredPercent} kadın oranı gerekli (mevcut: %{currentPercent})");
            }
        }

        // Erkek oranı kontrolü
        if (subRule.MaxMaleRatio.HasValue)
        {
            // Veri yoksa değerlendirilemez
            if (!context.MaleRatio.HasValue)
                return (false, string.Empty);

            if (context.MaleRatio.Value > subRule.MaxMaleRatio.Value)
            {
                var maxPercent = (int)(subRule.MaxMaleRatio.Value * 100);
                var currentPercent = (int)(context.MaleRatio.Value * 100);
                return (true, $"Maksimum %{maxPercent} erkek oranı aşıldı (mevcut: %{currentPercent})");
            }
        }

        return (false, string.Empty);
    }
}
