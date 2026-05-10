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
/// Kapora gereksinimi kuralı değerlendiricisi.
/// Belirli koşullarda kapora talep eder.
/// </summary>
public sealed class DepositRequiredRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public DepositRequiredRuleEvaluator(ILogger<DepositRequiredRuleEvaluator> logger)
        : base(logger)
    {
    }

    /// <inheritdoc />
    public override string RuleType => "deposit_required";

    /// <inheritdoc />
    public override Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Mekan kapora modülü aktif değilse atla
        if (!context.Venue.DepositEnabled)
        {
            Logger.LogDebug(
                "Kapora kuralı atlandı. Mekan kapora modülü aktif değil. VenueId={VenueId}",
                context.Venue.Id);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Koşulları parse et
        var conditions = ParseConditions<DepositRequiredConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Aksiyonları parse et
        var actions = ParseActions<DepositRequiredActions>(rule, ExpectedVersion);
        if (actions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Scope kontrolü
        if (!IsScopeMatched(conditions.Scopes, context))
        {
            Logger.LogDebug(
                "Kapora kuralı atlandı. Scope eşleşmedi. RuleId={RuleId}",
                rule.Id);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Kapora tutarını hesapla
        decimal depositAmount;
        if (actions.UseVenueDefault)
        {
            depositAmount = context.Venue.DepositAmount ?? 0m;
            if (context.Venue.DepositPerPerson)
            {
                depositAmount *= context.Reservation.PartySize;
            }
        }
        else
        {
            depositAmount = actions.Amount ?? 0m;
            if (actions.PerPerson)
            {
                depositAmount *= context.Reservation.PartySize;
            }
        }

        if (depositAmount <= 0)
        {
            Logger.LogDebug(
                "Kapora kuralı atlandı. Hesaplanan tutar 0. RuleId={RuleId}",
                rule.Id);
            return Task.FromResult<RuleOutcome?>(null);
        }

        var payload = JsonSerializer.Serialize(new { depositAmount });
        var message = $"Bu rezervasyon için {depositAmount:N2} TL kapora gereklidir.";

        Logger.LogInformation(
            "Kapora gereksinimi belirlendi. RuleId={RuleId}, Amount={Amount}",
            rule.Id, depositAmount);

        var outcome = CreateOutcome(rule, RuleActionType.Deposit, message, payload);
        return Task.FromResult<RuleOutcome?>(outcome);
    }

    /// <summary>
    /// Scope koşullarını kontrol eder.
    /// </summary>
    private bool IsScopeMatched(DepositScopes? scopes, ReservationContext context)
    {
        // Scope tanımlı değilse her zaman geçerli
        if (scopes == null)
            return true;

        var reservedFor = context.Reservation.ReservedFor;

        // Gün kontrolü
        if (scopes.Days != null && scopes.Days.Length > 0)
        {
            var dayName = reservedFor.DayOfWeek.ToString();
            if (!scopes.Days.Contains(dayName, StringComparer.OrdinalIgnoreCase))
            {
                Logger.LogDebug(
                    "Scope kontrolü: Gün eşleşmedi. Day={Day}, AllowedDays={AllowedDays}",
                    dayName, string.Join(",", scopes.Days));
                return false;
            }
        }

        // Saat kontrolü
        if (scopes.Times != null && scopes.Times.Length > 0)
        {
            var timeString = reservedFor.ToString("HH:00");
            if (!scopes.Times.Contains(timeString, StringComparer.OrdinalIgnoreCase))
            {
                Logger.LogDebug(
                    "Scope kontrolü: Saat eşleşmedi. Time={Time}, AllowedTimes={AllowedTimes}",
                    timeString, string.Join(",", scopes.Times));
                return false;
            }
        }

        // Masa kontrolü
        if (scopes.TableIds != null && scopes.TableIds.Length > 0 && context.Table != null)
        {
            var tableIdString = context.Table.Id.ToString();
            if (!scopes.TableIds.Contains(tableIdString, StringComparer.OrdinalIgnoreCase))
            {
                Logger.LogDebug(
                    "Scope kontrolü: Masa eşleşmedi. TableId={TableId}",
                    tableIdString);
                return false;
            }
        }

        // Minimum kişi sayısı kontrolü
        if (scopes.MinPartySize.HasValue &&
            context.Reservation.PartySize < scopes.MinPartySize.Value)
        {
            Logger.LogDebug(
                "Scope kontrolü: Kişi sayısı yetersiz. PartySize={PartySize}, Min={Min}",
                context.Reservation.PartySize, scopes.MinPartySize.Value);
            return false;
        }

        return true;
    }
}
