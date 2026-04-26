using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Staff;
using Tablewise.Domain.Exceptions;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.Staff.Queries;

/// <summary>
/// Davet önizleme sorgusu handler'ı.
/// </summary>
public sealed class GetInvitationPreviewQueryHandler : IRequestHandler<GetInvitationPreviewQuery, InvitationPreviewDto>
{
    private readonly TablewiseDbContext _dbContext;

    /// <summary>
    /// GetInvitationPreviewQueryHandler constructor.
    /// </summary>
    public GetInvitationPreviewQueryHandler(TablewiseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<InvitationPreviewDto> Handle(GetInvitationPreviewQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _dbContext.UserInvitations
            .Include(inv => inv.Tenant)
            .Include(inv => inv.InviterUser)
            .Where(inv => inv.Token == request.Token && !inv.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (invitation == null)
        {
            throw new BusinessRuleException(
                "Geçersiz davet linki.",
                "INVALID_INVITATION_TOKEN");
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Davet linkinin süresi dolmuş.",
                "INVITATION_EXPIRED");
        }

        if (invitation.AcceptedAt.HasValue)
        {
            throw new BusinessRuleException(
                "Bu davet zaten kabul edilmiş.",
                "INVITATION_ALREADY_ACCEPTED");
        }

        return new InvitationPreviewDto
        {
            Email = invitation.Email,
            TenantName = invitation.Tenant?.Name ?? "Unknown",
            InvitedBy = invitation.InviterUser != null
                ? $"{invitation.InviterUser.FirstName} {invitation.InviterUser.LastName}".Trim()
                : "Unknown",
            Role = invitation.Role.ToString(),
            InvitedAt = invitation.InvitedAt,
            ExpiresAt = invitation.ExpiresAt
        };
    }
}
