using MediatR;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Personel rol güncelleme komutu.
/// Sadece Owner kullanabilir.
/// </summary>
public sealed record UpdateStaffRoleCommand : IRequest
{
    /// <summary>
    /// Kullanıcı ID'si.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Yeni rol.
    /// </summary>
    public required UserRole NewRole { get; init; }
}
