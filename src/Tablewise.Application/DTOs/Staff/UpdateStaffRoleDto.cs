using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Staff;

/// <summary>
/// Personel rol güncelleme DTO'su.
/// </summary>
public sealed record UpdateStaffRoleDto
{
    /// <summary>
    /// Yeni rol.
    /// </summary>
    public required UserRole Role { get; init; }
}
