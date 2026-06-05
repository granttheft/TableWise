namespace Tablewise.Application.DTOs.Platform;

public record PlatformStatsDto
{
    public int TotalTenants { get; init; }
    public int ActiveTenants { get; init; }
    public int TrialTenants { get; init; }
    public int SuspendedTenants { get; init; }
    public int NewTenantsThisMonth { get; init; }
    public decimal Mrr { get; init; }
}
