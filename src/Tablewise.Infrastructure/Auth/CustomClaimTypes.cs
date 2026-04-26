namespace Tablewise.Infrastructure.Auth;

/// <summary>
/// Custom claim type sabitleri.
/// JWT token'larda kullanılan özel claim tipleri.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// Tenant ID claim.
    /// </summary>
    public const string TenantId = "tenant_id";

    /// <summary>
    /// User role claim.
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// Plan tier claim.
    /// </summary>
    public const string PlanTier = "plan_tier";
}
