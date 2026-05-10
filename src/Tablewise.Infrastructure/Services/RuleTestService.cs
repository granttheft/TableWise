using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.Services;

/// <summary>
/// Kural test servisi implementation.
/// Faz 3'te gerçek kural motoru ile çalışacak, şimdilik stub.
/// </summary>
public sealed class RuleTestService : IRuleTestService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RuleTestService> _logger;

    /// <summary>
    /// RuleTestService constructor.
    /// </summary>
    public RuleTestService(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ILogger<RuleTestService> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RuleEvaluationResult> TestRuleAsync(
        Guid ruleId,
        TestRuleRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantContext.TenantId;

        // Kuralı getir (validation için)
        var rule = await _dbContext.Rules
            .Where(r => r.Id == ruleId && r.TenantId == tenantId && !r.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rule == null)
        {
            _logger.LogWarning("Kural bulunamadı: RuleId={RuleId}", ruleId);
            return RuleEvaluationResult.Block("Kural bulunamadı.");
        }

        // Faz 3'te gerçek kural motoru ile değerlendirilecek
        // Şimdilik stub response dön
        _logger.LogInformation(
            "Kural test simüle edildi: RuleId={RuleId}, Name={Name}, PartySize={PartySize}",
            rule.Id, rule.Name, request.PartySize);

        // Stub sonuç - her zaman izin ver ama uyarı göster
        return new RuleEvaluationResult
        {
            IsAllowed = true,
            BlockReason = null,
            DiscountPercent = null,
            RequiresDeposit = false,
            DepositAmount = null,
            AppliedRules =
            [
                new AppliedRuleSnapshot
                {
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    ActionType = "Test",
                    ActionParams = new Dictionary<string, object>
                    {
                        { "ruleType", rule.RuleType },
                        { "priority", rule.Priority }
                    }
                }
            ],
            Warnings =
            [
                "⚠️ Kural motoru henüz implement edilmedi (Faz 3).",
                $"Kural: {rule.Name} ({rule.RuleType})",
                "Test sonucu simüle edilmiştir.",
                $"Girilen parametreler: {request.PartySize} kişi, {request.ReservedFor:yyyy-MM-dd HH:mm}"
            ]
        };
    }
}
