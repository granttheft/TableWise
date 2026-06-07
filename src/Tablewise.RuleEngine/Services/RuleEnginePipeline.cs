using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Interfaces;
using Tablewise.RuleEngine.Results;

namespace Tablewise.RuleEngine.Services;

/// <summary>
/// Kural motoru ana pipeline implementation.
/// Redis cache ile kuralları cache'ler ve sırayla değerlendirir.
/// </summary>
public sealed class RuleEnginePipeline : IRuleEnginePipeline
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly IRuleTypeEvaluatorFactory _evaluatorFactory;
    private readonly ILogger<RuleEnginePipeline> _logger;

    /// <summary>
    /// Redis cache TTL süresi (5 dakika).
    /// </summary>
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Constructor.
    /// </summary>
    public RuleEnginePipeline(
        IApplicationDbContext dbContext,
        ICacheService cacheService,
        IRuleTypeEvaluatorFactory evaluatorFactory,
        ILogger<RuleEnginePipeline> logger)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _evaluatorFactory = evaluatorFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PipelineResult> ExecuteAsync(
        ReservationContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new PipelineResult();
        var tenantId = context.Tenant.Id;
        var venueId = context.Venue.Id;

        _logger.LogInformation(
            "Kural pipeline başlatıldı. TenantId={TenantId}, VenueId={VenueId}, PartySize={PartySize}",
            tenantId, venueId, context.Reservation.PartySize);

        // Kuralları al (cache veya DB)
        var rules = await GetRulesAsync(tenantId, venueId, cancellationToken)
            .ConfigureAwait(false);
        var triggeredRuleIds = new List<Guid>();

        if (rules.Count == 0)
        {
            _logger.LogDebug("Tenant {TenantId} için aktif kural bulunamadı", tenantId);
            result.AddInfo("Aktif kural bulunamadı.");
            return result;
        }

        _logger.LogDebug(
            "{RuleCount} kural bulundu, değerlendirme başlıyor",
            rules.Count);

        // Her kuralı sırayla değerlendir (priority sırası korunmalı - paralel değil)
        foreach (var rule in rules)
        {
            try
            {
                var outcome = await EvaluateRuleAsync(rule, context, cancellationToken)
                    .ConfigureAwait(false);

                if (outcome == null)
                {
                    // Kural tetiklenmedi, devam et
                    continue;
                }

                // Outcome'u sonuca ekle
                result.AddOutcome(outcome);
                ProcessOutcome(result, outcome, rule);
                triggeredRuleIds.Add(rule.Id);

                // Block durumunda pipeline'ı durdur
                if (outcome.ActionType == RuleActionType.Block)
                {
                    result.IsBlocked = true;
                    result.BlockReason = outcome.Message ?? $"Kural tarafından engellendi: {rule.Name}";

                    _logger.LogInformation(
                        "Rezervasyon engellendi. Kural: {RuleName} ({RuleId}), Neden: {Reason}",
                        rule.Name, rule.Id, result.BlockReason);

                    // Pipeline durdur
                    break;
                }
            }
            catch (Exception ex)
            {
                // Tek bir kural hatası pipeline'ı durdurmaz
                _logger.LogError(ex,
                    "Kural değerlendirme hatası. RuleId={RuleId}, RuleName={RuleName}",
                    rule.Id, rule.Name);
                result.AddWarning($"Kural '{rule.Name}' değerlendirilirken hata oluştu.");
            }
        }

        // Tetiklenen kuralların TimesTriggered sayacını güncelle (fire-and-forget, hata pipeline'ı durdurmaz)
        if (triggeredRuleIds.Count > 0)
        {
            try
            {
                var now = DateTime.UtcNow;
                var ids = triggeredRuleIds;
                // Tracked load ederek güncelle
                var trackedRules = await _dbContext.Rules
                    .Where(r => ids.Contains(r.Id))
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                foreach (var r in trackedRules)
                {
                    r.TimesTriggered++;
                    r.UpdatedAt = now;
                }
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TimesTriggered güncellenirken hata oluştu, pipeline etkilenmedi.");
            }
        }

        _logger.LogInformation(
            "Kural pipeline tamamlandı. IsBlocked={IsBlocked}, OutcomeCount={OutcomeCount}, " +
            "TotalDiscount={Discount}%, RequiresDeposit={RequiresDeposit}",
            result.IsBlocked, result.Outcomes.Count,
            result.TotalDiscountPercent, result.RequiresDeposit);

        return result;
    }

    /// <summary>
    /// Redis cache veya DB'den kuralları alır.
    /// </summary>
    private async Task<List<Rule>> GetRulesAsync(
        Guid tenantId,
        Guid venueId,
        CancellationToken cancellationToken)
    {
        var cacheKey = RuleEngineCacheKeys.RulesForVenue(tenantId, venueId);

        // Cache'te var mı?
        var cachedRules = await _cacheService
            .GetAsync<List<CachedRule>>(cacheKey, cancellationToken)
            .ConfigureAwait(false);

        if (cachedRules != null && cachedRules.Count > 0)
        {
            _logger.LogDebug("Kurallar cache'ten alındı. Key={CacheKey}", cacheKey);
            return cachedRules.Select(MapToRule).ToList();
        }

        // DB'den al
        var rules = await _dbContext.Rules
            .AsNoTracking()
            .Where(r =>
                r.TenantId == tenantId &&
                (r.VenueId == venueId || r.VenueId == null) && // Tenant-wide kurallar dahil
                r.IsActive &&
                !r.IsDeleted)
            .OrderBy(r => r.Priority) // 1 = en yüksek öncelik
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Cache'e yaz
        if (rules.Count > 0)
        {
            var toCache = rules.Select(r => new CachedRule
            {
                Id = r.Id,
                VenueId = r.VenueId,
                Name = r.Name,
                Description = r.Description,
                RuleType = r.RuleType,
                ConditionsJson = r.ConditionsJson,
                ActionsJson = r.ActionsJson,
                Priority = r.Priority,
                TriggerType = r.TriggerType,
                IsActive = r.IsActive,
                ApplicableTimeSlots = r.ApplicableTimeSlots
            }).ToList();

            await _cacheService
                .SetAsync(cacheKey, toCache, CacheTtl, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug(
                "Kurallar cache'e yazıldı. Key={CacheKey}, Count={Count}, TTL={Ttl}",
                cacheKey, rules.Count, CacheTtl);
        }

        return rules;
    }

    /// <summary>
    /// Tek bir kuralı değerlendirir.
    /// </summary>
    private async Task<RuleOutcome?> EvaluateRuleAsync(
        Rule rule,
        ReservationContext context,
        CancellationToken cancellationToken)
    {
        // Evaluator'ı al
        var evaluator = _evaluatorFactory.GetFor(rule.RuleType);

        if (evaluator == null)
        {
            _logger.LogWarning(
                "Kural tipi için evaluator bulunamadı. RuleType={RuleType}, RuleId={RuleId}",
                rule.RuleType, rule.Id);
            return null;
        }

        // Değerlendir
        return await evaluator
            .EvaluateAsync(rule, context, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Outcome'u işler ve sonuca yansıtır.
    /// </summary>
    private static void ProcessOutcome(PipelineResult result, RuleOutcome outcome, Rule rule)
    {
        switch (outcome.ActionType)
        {
            case RuleActionType.Discount:
                // Discount payload'dan yüzdeyi çıkar ve topla
                if (TryParseDiscountPercent(outcome.Payload, out var discountPercent))
                {
                    result.TotalDiscountPercent += discountPercent;
                }
                break;

            case RuleActionType.Deposit:
                result.RequiresDeposit = true;
                if (TryParseDepositAmount(outcome.Payload, out var depositAmount))
                {
                    // En yüksek kapora tutarını al
                    if (!result.DepositAmount.HasValue || depositAmount > result.DepositAmount)
                    {
                        result.DepositAmount = depositAmount;
                    }
                }
                break;

            case RuleActionType.Warn:
                if (!string.IsNullOrEmpty(outcome.Message))
                {
                    result.AddWarning(outcome.Message);
                }
                break;

            case RuleActionType.Suggest:
                if (!string.IsNullOrEmpty(outcome.Payload))
                {
                    result.PreferredPosition = outcome.Payload;
                }
                break;

            case RuleActionType.Allow:
                // Özel işlem yok
                break;

            case RuleActionType.Block:
                // ExecuteAsync'te işleniyor
                break;

            case RuleActionType.Redirect:
                // TODO: v2'de redirect mantığı
                break;
        }
    }

    /// <summary>
    /// Discount payload'dan yüzde değerini çıkarır.
    /// </summary>
    private static bool TryParseDiscountPercent(string? payload, out decimal percent)
    {
        percent = 0;
        if (string.IsNullOrEmpty(payload))
            return false;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("discountPercent", out var prop))
            {
                percent = prop.GetDecimal();
                return true;
            }
        }
        catch
        {
            // Parse hatası - ignore
        }

        return false;
    }

    /// <summary>
    /// Deposit payload'dan tutarı çıkarır.
    /// </summary>
    private static bool TryParseDepositAmount(string? payload, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrEmpty(payload))
            return false;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("depositAmount", out var prop))
            {
                amount = prop.GetDecimal();
                return true;
            }
        }
        catch
        {
            // Parse hatası - ignore
        }

        return false;
    }

    /// <summary>
    /// CachedRule'dan Rule'a dönüştürür.
    /// </summary>
    private static Rule MapToRule(CachedRule cached) => new()
    {
        Id = cached.Id,
        VenueId = cached.VenueId,
        Name = cached.Name,
        Description = cached.Description,
        RuleType = cached.RuleType,
        ConditionsJson = cached.ConditionsJson,
        ActionsJson = cached.ActionsJson,
        Priority = cached.Priority,
        TriggerType = cached.TriggerType,
        IsActive = cached.IsActive,
        ApplicableTimeSlots = cached.ApplicableTimeSlots
    };

    /// <summary>
    /// Redis cache için hafif kural modeli.
    /// Navigation property'ler olmadan.
    /// </summary>
    private sealed class CachedRule
    {
        public Guid Id { get; init; }
        public Guid? VenueId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string RuleType { get; init; } = string.Empty;
        public string ConditionsJson { get; init; } = "{}";
        public string ActionsJson { get; init; } = "{}";
        public int Priority { get; init; }
        public RuleTrigger TriggerType { get; init; }
        public bool IsActive { get; init; }
        public string? ApplicableTimeSlots { get; init; }
    }
}
