using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Platform;

public record TenantSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public PlanStatus PlanStatus { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ReservationCountThisMonth { get; init; }
    public bool IsActive { get; init; }
}

public record PlatformNoteDto
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public string CreatedByEmail { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record TenantDetailDto : TenantSummaryDto
{
    public string Slug { get; init; } = string.Empty;
    public DateTime? TrialEndsAt { get; init; }
    public DateTime? PlanRenewsAt { get; init; }
    public int VenueCount { get; init; }
    public int UserCount { get; init; }
    public IReadOnlyList<PlatformNoteDto> Notes { get; init; } = [];
    // Faz 8 CRM bağımlılığı — rezervasyon geçmişi burada null
    public object? ReservationHistory { get; init; } = null;
    public string? CustomLimitsJson { get; init; }
}

public record UpdateTenantCustomLimitsDto(
    int? MaxVenues,
    int? MaxTables,
    int? MaxRules,
    int? MaxReservationsPerMonth,
    int? MaxStaffAccounts);
