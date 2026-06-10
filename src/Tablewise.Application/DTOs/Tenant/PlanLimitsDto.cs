namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Plan limitleri DTO'su
/// </summary>
public sealed class PlanLimitsDto
{
    public int? MaxTables { get; set; }
    public int CurrentTableCount { get; set; }
    public int? MaxRules { get; set; }
    public int CurrentRuleCount { get; set; }
    public int? MaxVenues { get; set; }
    public int CurrentVenueCount { get; set; }
    public int? MaxReservationsPerMonth { get; set; }
    public int CurrentReservationCount { get; set; }
    public bool HasCustomLimits { get; set; }
}
