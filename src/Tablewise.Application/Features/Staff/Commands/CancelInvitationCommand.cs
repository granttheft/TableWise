using MediatR;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Davet iptal komutu.
/// Sadece Owner kullanabilir.
/// </summary>
public sealed record CancelInvitationCommand : IRequest
{
    /// <summary>
    /// İptal edilecek davet ID'si.
    /// </summary>
    public required Guid InvitationId { get; init; }
}
