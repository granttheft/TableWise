using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Staff;

/// <summary>
/// Personel bilgisi DTO'su.
/// </summary>
public sealed record StaffMemberDto
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
    /// Aktif mi?
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Email doğrulandı mı?
    /// </summary>
    public required bool IsEmailVerified { get; init; }

    /// <summary>
    /// Davet ile mi katıldı?
    /// </summary>
    public DateTime? InvitedAt { get; init; }

    /// <summary>
    /// Son giriş tarihi.
    /// </summary>
    public DateTime? LastLoginAt { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
