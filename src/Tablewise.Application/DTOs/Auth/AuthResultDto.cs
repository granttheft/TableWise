using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Auth işlem sonucu DTO'su. Token + kullanıcı bilgisi.
/// </summary>
public sealed record AuthResultDto
{
    /// <summary>
    /// Token bilgileri.
    /// </summary>
    public required TokenResponseDto Tokens { get; init; }

    /// <summary>
    /// Kullanıcı bilgileri.
    /// </summary>
    public required UserInfoDto User { get; init; }

    /// <summary>
    /// Tenant bilgileri.
    /// </summary>
    public required TenantInfoDto Tenant { get; init; }
}

/// <summary>
/// Kullanıcı bilgisi DTO'su.
/// </summary>
public sealed record UserInfoDto
{
    /// <summary>
    /// Kullanıcı ID'si.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Email adresi.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Ad.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Soyad.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Rol.
    /// </summary>
    public required UserRole Role { get; init; }

    /// <summary>
    /// Email doğrulandı mı?
    /// </summary>
    public required bool IsEmailVerified { get; init; }
}

/// <summary>
/// Tenant bilgisi DTO'su.
/// </summary>
public sealed record TenantInfoDto
{
    /// <summary>
    /// Tenant ID'si.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// İşletme adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// URL slug.
    /// </summary>
    public required string Slug { get; init; }

    /// <summary>
    /// Plan seviyesi.
    /// </summary>
    public required PlanTier PlanTier { get; init; }

    /// <summary>
    /// Plan durumu.
    /// </summary>
    public required PlanStatus PlanStatus { get; init; }

    /// <summary>
    /// Deneme süresi bitiş tarihi.
    /// </summary>
    public DateTime? TrialEndsAt { get; init; }
}
