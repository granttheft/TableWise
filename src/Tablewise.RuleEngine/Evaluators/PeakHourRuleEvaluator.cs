using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Base;
using Tablewise.RuleEngine.Evaluators.Models;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Evaluators;

/// <summary>
/// Yoğun saat kuralı değerlendiricisi.
/// Belirli saat aralıklarında rezervasyonu engeller veya uyarı gösterir.
/// </summary>
public sealed class PeakHourRuleEvaluator : RuleEvaluatorBase
{
    private const int ExpectedVersion = 1;

    /// <summary>
    /// Constructor.
    /// </summary>
    public PeakHourRuleEvaluator(ILogger<PeakHourRuleEvaluator> logger)
        : base(logger)
    {
    }

    /// <inheritdoc />
    public override string RuleType => "peak_hour";

    /// <inheritdoc />
    public override Task<RuleOutcome?> EvaluateAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        // Koşulları parse et
        var conditions = ParseConditions<PeakHourConditions>(rule, ExpectedVersion);
        if (conditions == null)
            return Task.FromResult<RuleOutcome?>(null);

        // Aksiyonları parse et
        var actions = ParseActions<PeakHourActions>(rule, ExpectedVersion);
        if (actions == null)
            return Task.FromResult<RuleOutcome?>(null);

        var reservedFor = context.Reservation.ReservedFor;

        // Saat aralığı kontrolü
        if (!IsWithinTimeRange(reservedFor, conditions.StartTime, conditions.EndTime))
        {
            Logger.LogDebug(
                "Yoğun saat kuralı atlandı. Saat aralığı dışında. Time={Time}, Range={Start}-{End}",
                reservedFor.ToString("HH:mm"), conditions.StartTime, conditions.EndTime);
            return Task.FromResult<RuleOutcome?>(null);
        }

        // Gün kontrolü (opsiyonel)
        if (conditions.Days != null && conditions.Days.Length > 0)
        {
            var dayName = reservedFor.DayOfWeek.ToString();
            if (!conditions.Days.Contains(dayName, StringComparer.OrdinalIgnoreCase))
            {
                Logger.LogDebug(
                    "Yoğun saat kuralı atlandı. Gün eşleşmedi. Day={Day}",
                    dayName);
                return Task.FromResult<RuleOutcome?>(null);
            }
        }

        // Doluluk oranı kontrolü (opsiyonel)
        if (conditions.MinOccupancyPercent.HasValue)
        {
            var currentOccupancyPercent = context.CurrentOccupancyRate * 100;
            if (currentOccupancyPercent < conditions.MinOccupancyPercent.Value)
            {
                Logger.LogDebug(
                    "Yoğun saat kuralı atlandı. Doluluk oranı düşük. Current={Current}%, Min={Min}%",
                    currentOccupancyPercent, conditions.MinOccupancyPercent.Value);
                return Task.FromResult<RuleOutcome?>(null);
            }
        }

        // Aksiyon belirle
        RuleActionType actionType;
        if (actions.Block)
        {
            actionType = RuleActionType.Block;
            Logger.LogWarning(
                "Yoğun saat engeli uygulandı. RuleId={RuleId}, Time={Time}",
                rule.Id, reservedFor.ToString("HH:mm"));
        }
        else if (actions.Warn)
        {
            actionType = RuleActionType.Warn;
            Logger.LogInformation(
                "Yoğun saat uyarısı uygulandı. RuleId={RuleId}, Time={Time}",
                rule.Id, reservedFor.ToString("HH:mm"));
        }
        else
        {
            // Ne block ne warn - aksiyon yok
            return Task.FromResult<RuleOutcome?>(null);
        }

        var message = !string.IsNullOrEmpty(actions.Message)
            ? actions.Message
            : "Bu saat dilimi yoğun dönemdir.";

        var outcome = CreateOutcome(rule, actionType, message);
        return Task.FromResult<RuleOutcome?>(outcome);
    }

    /// <summary>
    /// Saatin belirtilen aralıkta olup olmadığını kontrol eder.
    /// </summary>
    private bool IsWithinTimeRange(DateTime dateTime, string startTimeStr, string endTimeStr)
    {
        if (!TimeSpan.TryParse(startTimeStr, out var startTime) ||
            !TimeSpan.TryParse(endTimeStr, out var endTime))
        {
            Logger.LogWarning(
                "Yoğun saat kuralı: Geçersiz saat formatı. Start={Start}, End={End}",
                startTimeStr, endTimeStr);
            return false;
        }

        var currentTime = dateTime.TimeOfDay;

        // Gece yarısını geçen aralık kontrolü (ör: 22:00 - 02:00)
        if (endTime < startTime)
        {
            return currentTime >= startTime || currentTime < endTime;
        }

        return currentTime >= startTime && currentTime < endTime;
    }
}
