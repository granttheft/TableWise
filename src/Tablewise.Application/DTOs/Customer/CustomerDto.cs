namespace Tablewise.Application.DTOs.Customer;

/// <summary>
/// Müşteri DTO'su
/// </summary>
public sealed class CustomerDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Tier { get; set; } = "Regular";
    public int? TotalVisits { get; set; }
    public DateTime? LastReservationDate { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? LastVisitedVenueName { get; set; }
}

/// <summary>
/// Tier güncelleme DTO'su
/// </summary>
public sealed class UpdateCustomerTierDto
{
    public string Tier { get; set; } = string.Empty;
}

/// <summary>
/// Blacklist güncelleme DTO'su
/// </summary>
public sealed class UpdateCustomerBlacklistDto
{
    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }
}

/// <summary>
/// Not güncelleme DTO'su
/// </summary>
public sealed class UpdateCustomerNotesDto
{
    public string? Notes { get; set; }
}
