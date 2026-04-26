using MediatR;
using Tablewise.Application.DTOs.Staff;

namespace Tablewise.Application.Features.Staff.Queries;

/// <summary>
/// Davet listesi sorgusu.
/// Sadece Owner kullanabilir.
/// </summary>
public sealed record ListInvitationsQuery : IRequest<List<InvitationDto>>
{
    /// <summary>
    /// Sadece bekleyen davetler mi? (default: false - tümü)
    /// </summary>
    public bool PendingOnly { get; init; }
}
