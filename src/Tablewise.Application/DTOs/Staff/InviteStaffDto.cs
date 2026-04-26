using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Staff;

/// <summary>
/// Personel davet isteği DTO'su.
/// </summary>
public sealed record InviteStaffDto
{
    /// <summary>
    /// Davet edilecek personelin email adresi.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Atanacak rol (sadece Staff veya Owner).
    /// </summary>
    public required UserRole Role { get; init; }

    /// <summary>
    /// Opsiyonel not/mesaj.
    /// </summary>
    public string? Message { get; init; }
}
