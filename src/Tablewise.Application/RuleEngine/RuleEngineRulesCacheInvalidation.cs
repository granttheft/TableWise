using Tablewise.Application.Interfaces;

namespace Tablewise.Application.RuleEngine;

/// <summary>
/// Invalidates Redis rule lists cached by RuleEnginePipeline (key prefix "rules:").
/// </summary>
public static class RuleEngineRulesCacheInvalidation
{
    /// <summary>
    /// Removes all cached rule sets for the tenant (all venue keys).
    /// </summary>
    public static Task InvalidateForTenantAsync(
        ICacheService cacheService,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return cacheService.RemoveByPatternAsync($"rules:{tenantId}:*", cancellationToken);
    }
}
