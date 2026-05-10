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
/// Büyük grup kuralı değerlendiricisi.
/// Büyük gruplar için masa birleşimi önerir veya uygun masa yoksa engeller.
/// </summary>
public sealed class LargeGroupRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public LargeGroupRuleEvaluator(ILogger<LargeGroupRuleEvaluator> logger)
        : base(logger)
    {
    }

    /// <inheritdoc />
    public override string RuleType => "large_group";

    /// <inheritdoc />
    public override Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Koşulları parse et
        var conditions = ParseConditions<LargeGroupConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Aksiyonları parse et
        var actions = ParseActions<LargeGroupActions>(rule, ExpectedVersion);
        if (actions == null)
            return Task.FromResult<RuleOutcome?>(null);

        var partySize = context.Reservation.PartySize;

        // Minimum kişi sayısı kontrolü
        if (partySize < conditions.MinPartySize)
        {
            Logger.LogDebug(
                "Büyük grup kuralı atlandı. PartySize={PartySize} < Min={Min}",
                partySize, conditions.MinPartySize);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Mevcut masa kapasitesi kontrolü
        if (context.Table != null && context.Table.Capacity >= partySize)
        {
            Logger.LogDebug(
                "Büyük grup kuralı atlandı. Masa kapasitesi yeterli. TableCapacity={Capacity}, PartySize={PartySize}",
                context.Table.Capacity, partySize);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Zaten masa birleşimi seçilmişse ve yeterliyse
        if (context.TableCombination != null &&
            context.TableCombination.CombinedCapacity >= partySize)
        {
            Logger.LogDebug(
                "Büyük grup kuralı atlandı. Masa birleşimi yeterli. CombinedCapacity={Capacity}, PartySize={PartySize}",
                context.TableCombination.CombinedCapacity, partySize);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Uygun masa birleşimi ara
        var suitableCombinations = context.Venue.TableCombinations
            .Where(tc => tc.IsActive && !tc.IsDeleted && tc.CombinedCapacity >= partySize)
            .OrderBy(tc => tc.CombinedCapacity)
            .ToList();

        if (suitableCombinations.Count > 0)
        {
            // Uygun birleşim bulundu - öner
            var bestCombination = suitableCombinations.First();
            var payload = JsonSerializer.Serialize(new
            {
                suggestedCombinationId = bestCombination.Id,
                suggestedCombinationName = bestCombination.Name,
                combinedCapacity = bestCombination.CombinedCapacity
            });

            var message = $"Grubunuz için {bestCombination.Name} masa birleşimi önerilmektedir.";

            Logger.LogInformation(
                "Büyük grup için masa birleşimi önerildi. RuleId={RuleId}, CombinationId={CombinationId}",
                rule.Id, bestCombination.Id);

            var outcome = CreateOutcome(rule, RuleActionType.Suggest, message, payload);
            return Task.FromResult<RuleOutcome?>(outcome);
        }

        // Uygun birleşim yok - engelle
        var blockMessage = !string.IsNullOrEmpty(actions.Message)
            ? actions.Message
            : $"{partySize} kişilik grup için uygun masa bulunamadı.";

        Logger.LogWarning(
            "Büyük grup için uygun masa bulunamadı. RuleId={RuleId}, PartySize={PartySize}",
            rule.Id, partySize);

        var blockOutcome = CreateOutcome(rule, RuleActionType.Block, blockMessage);
        return Task.FromResult<RuleOutcome?>(blockOutcome);
    }
}
