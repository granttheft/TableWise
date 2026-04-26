using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Tenant profil DTO'su.
/// </summary>
public sealed record TenantProfileDto
{
    /// <summary>
    /// Tenant ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// İşletme adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// URL slug (değiştirilemez).
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// Email.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Logo URL.
    /// </summary>
    public string? LogoUrl { get; init; }

    /// <summary>
    /// Plan seviyesi.
    /// </summary>
    public required PlanTier PlanTier { get; init; }

    /// <summary>
    /// Plan adı.
    /// </summary>
    public required string PlanName { get; init; }

    /// <summary>
    /// Plan durumu.
    /// </summary>
    public required PlanStatus PlanStatus { get; init; }

    /// <summary>
    /// Trial bitiş tarihi.
    /// </summary>
    public DateTime? TrialEndsAt { get; init; }

    /// <summary>
    /// Plan yenileme tarihi.
    /// </summary>
    public DateTime? PlanRenewsAt { get; init; }

    /// <summary>
    /// Email doğrulandı mı?
    /// </summary>
    public required bool IsEmailVerified { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
