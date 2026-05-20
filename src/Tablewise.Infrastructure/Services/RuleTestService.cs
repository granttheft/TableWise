using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Tablewise.RuleEngine.Evaluators;
using Tablewise.RuleEngine.Evaluators.Models;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Kural test servisi implementation.
/// Kuralları gerçek evaluator'lar ile test eder.
/// </summary>
public sealed class RuleTestService : IRuleTestService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IRuleTypeEvaluatorFactory _evaluatorFactory;
    private readonly ILogger<RuleTestService> _logger;

    /// <summary>
    /// RuleTestService constructor.
    /// </summary>
    public RuleTestService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        IRuleTypeEvaluatorFactory evaluatorFactory,
        ILogger<RuleTestService> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _evaluatorFactory = evaluatorFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RuleTestResultDto> TestRuleAsync(
        Guid ruleId,
        TestRuleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        var stopwatch = Stopwatch.StartNew();

        // Kuralı getir
        var rule = await _dbContext.Rules
            .Where(r => r.Id == ruleId && r.TenantId == tenantId && !r.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rule == null)
        {
            _logger.LogWarning("Kural bulunamadı: RuleId={RuleId}", ruleId);
            stopwatch.Stop();
            return new RuleTestResultDto
            {
                Triggered = false,
                ExecutionMs = (int)stopwatch.ElapsedMilliseconds
            };
        }

        // Evaluator'ı bul
        var evaluator = _evaluatorFactory.GetFor(rule.RuleType);
        if (evaluator == null)
        {
            _logger.LogWarning(
                "Kural tipi için evaluator bulunamadı: RuleType={RuleType}",
                rule.RuleType);
            stopwatch.Stop();
            return new RuleTestResultDto
            {
                Triggered = false,
                ExecutionMs = (int)stopwatch.ElapsedMilliseconds
            };
        }

        // Test context oluştur (DB'ye dokunmadan)
        var context = BuildReservationContext(request, tenantId);

        // Kuralı değerlendir
        var outcome = await evaluator.EvaluateAsync(rule, context, cancellationToken)
            .ConfigureAwait(false);

        stopwatch.Stop();

        // Sonucu oluştur
        var result = new RuleTestResultDto
        {
            Triggered = outcome != null,
            ExecutionMs = (int)stopwatch.ElapsedMilliseconds,
            Outcome = outcome != null
                ? new RuleOutcomeDto
                {
                    RuleId = outcome.RuleId,
                    RuleName = outcome.RuleName,
                    ActionType = outcome.ActionType.ToString(),
                    Message = outcome.Message,
                    Payload = outcome.Payload
                }
                : null
        };

        // CustomCondition için detaylı değerlendirme ekle
        if (evaluator is CustomConditionRuleEvaluator customEvaluator && 
            rule.RuleType == "custom_condition")
        {
            result = result with
            {
                ConditionEvaluations = GetConditionEvaluations(rule, context, customEvaluator)
            };
        }

        _logger.LogInformation(
            "Kural test edildi: RuleId={RuleId}, RuleType={RuleType}, Triggered={Triggered}, ExecutionMs={ExecutionMs}",
            rule.Id, rule.RuleType, result.Triggered, result.ExecutionMs);

        return result;
    }

    /// <inheritdoc />
    public async Task<RuleTestResultDto> TestDraftRuleAsync(
        TestDraftRuleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;
        var stopwatch = Stopwatch.StartNew();

        var rule = new Rule
        {
            Id = Guid.Empty,
            TenantId = tenantId,
            Name = string.IsNullOrWhiteSpace(request.RuleName) ? "Taslak kural" : request.RuleName.Trim(),
            RuleType = request.RuleType,
            ConditionsJson = request.ConditionsJson,
            ActionsJson = request.ActionsJson,
            TriggerType = RuleTrigger.OnReservationCreate,
            Priority = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var evaluator = _evaluatorFactory.GetFor(rule.RuleType);
        if (evaluator == null)
        {
            stopwatch.Stop();
            return new RuleTestResultDto
            {
                Triggered = false,
                ExecutionMs = (int)stopwatch.ElapsedMilliseconds
            };
        }

        var context = BuildReservationContext(request.Context, tenantId);
        var outcome = await evaluator.EvaluateAsync(rule, context, cancellationToken)
            .ConfigureAwait(false);

        stopwatch.Stop();

        var result = new RuleTestResultDto
        {
            Triggered = outcome != null,
            ExecutionMs = (int)stopwatch.ElapsedMilliseconds,
            Outcome = outcome != null
                ? new RuleOutcomeDto
                {
                    RuleId = Guid.Empty,
                    RuleName = rule.Name,
                    ActionType = outcome.ActionType.ToString(),
                    Message = outcome.Message,
                    Payload = outcome.Payload
                }
                : null
        };

        if (evaluator is CustomConditionRuleEvaluator customEvaluator &&
            rule.RuleType.Equals("custom_condition", StringComparison.OrdinalIgnoreCase))
        {
            result = result with
            {
                ConditionEvaluations = GetConditionEvaluations(rule, context, customEvaluator)
            };
        }

        return result;
    }

    /// <summary>
    /// TestRuleRequestDto'dan ReservationContext oluşturur.
    /// In-memory, DB'ye dokunmaz.
    /// </summary>
    private ReservationContext BuildReservationContext(TestRuleRequestDto request, Guid tenantId)
    {
        var venueId = Guid.NewGuid();
        var tableId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Rezervasyon zamanını hesapla
        var reservedFor = CalculateReservedFor(request);

        // Tenant (mock)
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Test Tenant",
            Slug = "test",
            Email = "test@test.com"
        };

        // Venue (mock)
        var venue = new Venue
        {
            Id = venueId,
            TenantId = tenantId,
            Name = "Test Venue",
            OpeningTime = TimeSpan.FromHours(10),
            ClosingTime = TimeSpan.FromHours(23)
        };

        // Table (mock, opsiyonel)
        Table? table = null;
        if (request.TableCapacity.HasValue)
        {
            var tableLocation = !string.IsNullOrEmpty(request.TableLocation) &&
                Enum.TryParse<TableLocation>(request.TableLocation, true, out var parsedLocation)
                    ? parsedLocation
                    : TableLocation.Indoor;

            table = new Table
            {
                Id = tableId,
                TenantId = tenantId,
                VenueId = venueId,
                Name = "Test Table",
                Capacity = request.TableCapacity.Value,
                Location = tableLocation,
                IsActive = true
            };
        }

        // Customer (mock, opsiyonel)
        Customer? customer = null;
        if (!string.IsNullOrEmpty(request.CustomerTier))
        {
            var tier = Enum.TryParse<CustomerTier>(request.CustomerTier, true, out var parsedTier)
                ? parsedTier
                : CustomerTier.Regular;

            customer = new Customer
            {
                Id = customerId,
                TenantId = tenantId,
                FullName = "Test Customer",
                Phone = "5551234567",
                Tier = tier,
                TotalVisits = request.CustomerTotalVisits ?? 0
            };
        }

        // Reservation (mock)
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = venueId,
            TableId = table?.Id,
            PartySize = request.PartySize,
            ReservedFor = reservedFor,
            GuestName = "Test Guest",
            GuestPhone = "5551234567",
            CustomerId = customer?.Id
        };

        return new ReservationContext
        {
            Tenant = tenant,
            Venue = venue,
            Reservation = reservation,
            Table = table,
            Customer = customer,
            DaysInAdvance = request.DaysInAdvance,
            CurrentOccupancyRate = request.VenueOccupancy,
            MaleCount = request.MaleCount,
            FemaleCount = request.FemaleCount,
            GroupComposition = request.GroupComposition,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Rezervasyon zamanını hesaplar.
    /// </summary>
    private static DateTime CalculateReservedFor(TestRuleRequestDto request)
    {
        var baseDate = DateTime.UtcNow.Date.AddDays(request.DaysInAdvance);

        // DayOfWeek belirtilmişse o güne ayarla
        if (!string.IsNullOrEmpty(request.DayOfWeek) &&
            Enum.TryParse<DayOfWeek>(request.DayOfWeek, true, out var targetDay))
        {
            var currentDay = baseDate.DayOfWeek;
            var daysToAdd = ((int)targetDay - (int)currentDay + 7) % 7;
            if (daysToAdd == 0 && baseDate <= DateTime.UtcNow.Date)
                daysToAdd = 7;
            baseDate = baseDate.AddDays(daysToAdd);
        }

        return baseDate.AddHours(request.Hour);
    }

    /// <summary>
    /// CustomCondition kuralı için koşul değerlendirmelerini alır.
    /// </summary>
    private List<ConditionEvaluationDto>? GetConditionEvaluations(
        Rule rule,
        ReservationContext context,
        CustomConditionRuleEvaluator evaluator)
    {
        try
        {
            var conditions = System.Text.Json.JsonSerializer.Deserialize<CustomConditionConditions>(
                rule.ConditionsJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (conditions?.Conditions == null || conditions.Conditions.Length == 0)
                return null;

            var (_, evaluations) = evaluator.EvaluateConditions(conditions, context);

            return evaluations.Select(e => new ConditionEvaluationDto
            {
                Field = e.Field,
                Op = e.Op,
                ExpectedValue = e.ExpectedValue,
                ActualValue = e.ActualValue,
                Result = e.Result
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Koşul değerlendirmeleri alınamadı: RuleId={RuleId}", rule.Id);
            return null;
        }
    }
}
