using MediatR;
using Tablewise.Application.DTOs.Staff;

namespace Tablewise.Application.Features.Staff.Queries;

/// <summary>
/// Davet önizleme sorgusu.
/// Public endpoint - authentication gerektirmez.
/// </summary>
public sealed record GetInvitationPreviewQuery : IRequest<InvitationPreviewDto>
{
    /// <summary>
    /// Davet token'ı.
    /// </summary>
    public required string Token { get; init; }
}
