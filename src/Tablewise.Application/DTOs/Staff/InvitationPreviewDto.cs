namespace Tablewise.Application.DTOs.Staff;

/// <summary>
/// Davet önizleme DTO'su (token ile davet bilgilerini görmek için).
/// </summary>
public sealed record InvitationPreviewDto
{
    /// <summary>
    /// Davet edilen email adresi.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Tenant/işletme adı.
    /// </summary>
    public required string TenantName { get; init; }

    /// <summary>
    /// Davet eden kişi adı.
    /// </summary>
    public required string InvitedBy { get; init; }

    /// <summary>
    /// Atanacak rol.
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Davet tarihi.
    /// </summary>
    public required DateTime InvitedAt { get; init; }

    /// <summary>
    /// Son kullanma tarihi.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }
}
