using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Base;
using Tablewise.RuleEngine.Evaluators.Models;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Evaluators;

/// <summary>
/// Masa bekleme süresi kuralı değerlendiricisi.
/// Bir rezervasyon bittikten sonra aynı masaya yeni rezervasyon için belirli süre bekletir.
/// </summary>
public sealed class TableCooldownRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;
    private readonly IApplicationDbContext _dbContext;

    /// <summary>
    /// Constructor.
    /// </summary>
    public TableCooldownRuleEvaluator(
        ILogger<TableCooldownRuleEvaluator> logger,
        IApplicationDbContext dbContext)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public override string RuleType => "table_cooldown";

    /// <inheritdoc />
    public override async Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Masa seçilmemişse kural uygulanamaz
        if (context.Table == null)
        {
            Logger.LogDebug(
                "Masa bekleme kuralı atlandı. Masa seçilmemiş.");
            return null;
        }

        // Koşulları parse et
        var conditions = ParseConditions<TableCooldownConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return null;

        // Aksiyonları parse et
        var actions = ParseActions<TableCooldownActions>(rule, ExpectedVersion);
        if (actions == null)
            return null;

        var reservedFor = context.Reservation.ReservedFor;
        var tableId = context.Table.Id;
        var reservationDate = reservedFor.Date;

        // Aynı masanın o gün son tamamlanan/onaylanan rezervasyonunu bul
        var lastReservation = await _dbContext.Reservations
            .AsNoTracking()
            .Where(r =>
                r.TableId == tableId &&
                r.ReservedFor.Date == reservationDate &&
                (r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Completed) &&
                !r.IsDeleted &&
                r.Id != context.Reservation.Id) // Kendisi hariç
            .OrderByDescending(r => r.EndTime)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (lastReservation == null)
        {
            Logger.LogDebug(
                "Masa bekleme kuralı atlandı. O gün için önceki rezervasyon yok. TableId={TableId}",
                tableId);
            return null;
        }

        // Cooldown süresi kontrolü
        var cooldownEnd = lastReservation.EndTime.AddMinutes(conditions.CooldownMinutes);
        if (reservedFor >= cooldownEnd)
        {
            Logger.LogDebug(
                "Masa bekleme kuralı atlandı. Bekleme süresi dolmuş. LastEnd={LastEnd}, CooldownEnd={CooldownEnd}, ReservedFor={ReservedFor}",
                lastReservation.EndTime, cooldownEnd, reservedFor);
            return null;
        }

        // Bekleme süresi dolmamış - engelle
        var message = !string.IsNullOrEmpty(actions.Message)
            ? actions.Message
            : $"Bu masa için en erken {cooldownEnd:HH:mm} saatinden itibaren rezervasyon yapılabilir.";

        Logger.LogWarning(
            "Masa bekleme süresi ihlali. RuleId={RuleId}, TableId={TableId}, LastEnd={LastEnd}, CooldownEnd={CooldownEnd}, ReservedFor={ReservedFor}",
            rule.Id, tableId, lastReservation.EndTime, cooldownEnd, reservedFor);

        var outcome = CreateOutcome(rule, RuleActionType.Block, message);
        return outcome;
    }
}
