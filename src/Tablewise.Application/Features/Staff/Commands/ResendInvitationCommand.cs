using MediatR;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Davet tekrar gönder komutu.
/// Sadece Owner kullanabilir.
/// </summary>
public sealed record ResendInvitationCommand : IRequest
{
    /// <summary>
    /// Tekrar gönderilecek davet ID'si.
    /// </summary>
    public required Guid InvitationId { get; init; }
}
