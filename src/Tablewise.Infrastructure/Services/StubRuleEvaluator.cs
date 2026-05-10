using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Kural değerlendiricisi. IRuleEnginePipeline'ı kullanarak kural motorunu çalıştırır.
/// Application layer (RuleEvaluationContext) ile RuleEngine layer (ReservationContext) arasında mapping yapar.
/// </summary>
public sealed class StubRuleEvaluator : IRuleEvaluator
{
    private readonly IRuleEnginePipeline _pipeline;
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<StubRuleEvaluator> _logger;

    /// <summary>
    /// StubRuleEvaluator constructor.
    /// </summary>
    public StubRuleEvaluator(
        IRuleEnginePipeline pipeline,
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ILogger<StubRuleEvaluator> logger)
    {
        _pipeline = pipeline;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RuleEvaluationResult> EvaluateAsync(
        RuleEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Application context -> RuleEngine context mapping
            var reservationContext = await BuildReservationContextAsync(context, cancellationToken)
                .ConfigureAwait(false);

            // Pipeline çalıştır
            var pipelineResult = await _pipeline.ExecuteAsync(reservationContext, cancellationToken)
                .ConfigureAwait(false);

            // PipelineResult -> RuleEvaluationResult mapping
            return MapToRuleEvaluationResult(pipelineResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kural değerlendirme hatası. VenueId={VenueId}", context.VenueId);
            
            // Hata durumunda izin ver (fail-open yaklaşımı)
            return RuleEvaluationResult.Allow();
        }
    }

    /// <summary>
    /// RuleEvaluationContext'i ReservationContext'e dönüştürür.
    /// </summary>
    private async Task<ReservationContext> BuildReservationContextAsync(
        RuleEvaluationContext context,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Venue load (tenant included)
        var venue = await _dbContext.Venues
            .Include(v => v.Tenant)
            .Where(v => v.Id == context.VenueId && !v.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new InvalidOperationException($"Venue not found: {context.VenueId}");
        }

        // Customer load (opsiyonel)
        Customer? customer = null;
        if (!string.IsNullOrEmpty(context.CustomerEmail))
        {
            customer = await _dbContext.Customers
                .Where(c => c.TenantId == tenantId &&
                           c.Email == context.CustomerEmail &&
                           !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        else if (!string.IsNullOrEmpty(context.CustomerPhone))
        {
            var normalizedPhone = context.CustomerPhone.Replace("+90", "").Replace(" ", "");
            customer = await _dbContext.Customers
                .Where(c => c.TenantId == tenantId &&
                           c.Phone.Replace("+90", "").Replace(" ", "") == normalizedPhone &&
                           !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        // Table load (opsiyonel)
        Table? table = null;
        if (context.TableId.HasValue)
        {
            table = await _dbContext.Tables
                .Where(t => t.Id == context.TableId.Value && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        // TableCombination load (opsiyonel)
        TableCombination? tableCombination = null;
        if (context.TableCombinationId.HasValue)
        {
            tableCombination = await _dbContext.TableCombinations
                .Where(tc => tc.Id == context.TableCombinationId.Value && !tc.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        // Mock reservation entity (sadece context için, DB'ye kaydedilmeyecek)
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = venue.Id,
            TableId = context.TableId,
            TableCombinationId = context.TableCombinationId,
            CustomerId = customer?.Id,
            GuestName = "Evaluation",
            GuestEmail = context.CustomerEmail,
            GuestPhone = context.CustomerPhone ?? string.Empty,
            PartySize = context.PartySize,
            ReservedFor = context.ReservedFor,
            EndTime = context.ReservedFor.AddMinutes(venue.SlotDurationMinutes),
            Status = ReservationStatus.Pending,
            Source = ReservationSource.BookingUI
        };

        // Grup kompozisyonu mapping
        var (groupComposition, maleCount, femaleCount) = ExtractGroupCompositionData(context.CustomFieldAnswers);

        // DaysInAdvance hesapla
        var daysInAdvance = (int)(context.ReservedFor.Date - DateTime.UtcNow.Date).TotalDays;

        // ReservationContext oluştur
        return new ReservationContext
        {
            Tenant = venue.Tenant!,
            Venue = venue,
            Reservation = reservation,
            Table = table,
            TableCombination = tableCombination,
            Customer = customer,
            DaysInAdvance = daysInAdvance,
            CurrentOccupancyRate = 0.0, // Şimdilik 0, gerekirse hesaplanabilir
            MaleCount = maleCount,
            FemaleCount = femaleCount,
            GroupComposition = groupComposition,
            EvaluatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// CustomFieldAnswers'dan grup kompozisyonu bilgilerini çıkarır.
    /// </summary>
    private (string? GroupComposition, int? MaleCount, int? FemaleCount) ExtractGroupCompositionData(
        Dictionary<string, string>? customFieldAnswers)
    {
        if (customFieldAnswers == null || customFieldAnswers.Count == 0)
            return (null, null, null);

        // Grup kompozisyonu mapping
        string? groupComposition = null;
        if (customFieldAnswers.TryGetValue("group_composition", out var compValue) ||
            customFieldAnswers.TryGetValue("Grup Kompozisyonu", out compValue))
        {
            groupComposition = compValue switch
            {
                "Karma (Kadın+Erkek)" => "Mixed",
                "Sadece Erkek" => "AllMale",
                "Sadece Kadın" => "AllFemale",
                "Aile" => "Family",
                _ => null
            };
        }

        // Erkek sayısı
        int? maleCount = null;
        if (customFieldAnswers.TryGetValue("male_count", out var maleValue) ||
            customFieldAnswers.TryGetValue("Erkek Misafir Sayısı", out maleValue))
        {
            if (int.TryParse(maleValue, out var parsedMale))
                maleCount = parsedMale;
        }

        // Kadın sayısı
        int? femaleCount = null;
        if (customFieldAnswers.TryGetValue("female_count", out var femaleValue) ||
            customFieldAnswers.TryGetValue("Kadın Misafir Sayısı", out femaleValue))
        {
            if (int.TryParse(femaleValue, out var parsedFemale))
                femaleCount = parsedFemale;
        }

        return (groupComposition, maleCount, femaleCount);
    }

    /// <summary>
    /// PipelineResult'ı RuleEvaluationResult'a dönüştürür.
    /// </summary>
    private RuleEvaluationResult MapToRuleEvaluationResult(
        RuleEngine.Results.PipelineResult pipelineResult)
    {
        // Blocked ise
        if (pipelineResult.IsBlocked)
        {
            return RuleEvaluationResult.Block(
                pipelineResult.BlockReason ?? "Kural ihlali nedeniyle reddedildi.");
        }

        // Applied rules mapping
        var appliedRules = pipelineResult.Outcomes.Select(outcome => new AppliedRuleSnapshot
        {
            RuleId = outcome.RuleId,
            RuleName = outcome.RuleName,
            ActionType = outcome.ActionType.ToString(),
            ActionParams = outcome.Payload != null
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(outcome.Payload)
                : null
        }).ToList();

        return new RuleEvaluationResult
        {
            IsAllowed = true,
            DiscountPercent = pipelineResult.TotalDiscountPercent > 0
                ? pipelineResult.TotalDiscountPercent
                : null,
            RequiresDeposit = pipelineResult.RequiresDeposit,
            DepositAmount = pipelineResult.DepositAmount,
            AppliedRules = appliedRules,
            Warnings = pipelineResult.Warnings.ToList()
        };
    }
}
