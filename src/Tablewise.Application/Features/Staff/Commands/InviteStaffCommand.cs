using MediatR;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Personel davet komutu.
/// Sadece Owner rol

ü kullanabilir.
/// </summary>
public sealed record InviteStaffCommand : IRequest<Guid>
{
    /// <summary>
    /// Davet edilecek email adresi.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Atanacak rol.
    /// </summary>
    public required UserRole Role { get; init; }

    /// <summary>
    /// Opsiyonel mesaj.
    /// </summary>
    public string? Message { get; init; }
}
