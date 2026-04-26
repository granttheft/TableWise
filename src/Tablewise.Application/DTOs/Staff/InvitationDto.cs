using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Staff;

/// <summary>
/// Personel davet bilgisi DTO'su.
/// </summary>
public sealed record InvitationDto
{
    /// <summary>
    /// Davet ID'si.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Davet edilen email adresi.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Atanacak rol.
    /// </summary>
    public required UserRole Role { get; init; }

    /// <summary>
    /// Davet eden kullanıcı adı.
    /// </summary>
    public required string InvitedBy { get; init; }

    /// <summary>
    /// Davet tarihi.
    /// </summary>
    public required DateTime InvitedAt { get; init; }

    /// <summary>
    /// Son kullanma tarihi.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Kabul edildi mi?
    /// </summary>
    public bool IsAccepted { get; init; }

    /// <summary>
    /// Kabul edilme tarihi.
    /// </summary>
    public DateTime? AcceptedAt { get; init; }

    /// <summary>
    /// Süresi dolmuş mu?
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt && !IsAccepted;

    /// <summary>
    /// Beklemede mi?
    /// </summary>
    public bool IsPending => !IsAccepted && !IsExpired;
}
