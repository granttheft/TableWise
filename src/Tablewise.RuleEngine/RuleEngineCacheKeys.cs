namespace Tablewise.RuleEngine;

/// <summary>
/// Redis cache keys for rule engine (must stay aligned with invalidation in rule command handlers).
/// </summary>
public static class RuleEngineCacheKeys
{
    private const string RulesPrefix = "rules";

    /// <summary>
    /// Active rules list for a tenant/venue pair.
    /// </summary>
    public static string RulesForVenue(Guid tenantId, Guid venueId) => $"{RulesPrefix}:{tenantId}:{venueId}";
}
