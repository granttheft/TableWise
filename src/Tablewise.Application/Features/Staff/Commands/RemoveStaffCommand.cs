using MediatR;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Personel silme komutu (soft delete).
/// Sadece Owner kullanabilir.
/// </summary>
public sealed record RemoveStaffCommand : IRequest
{
    /// <summary>
    /// Silinecek kullanıcı ID'si.
    /// </summary>
    public required Guid UserId { get; init; }
}
