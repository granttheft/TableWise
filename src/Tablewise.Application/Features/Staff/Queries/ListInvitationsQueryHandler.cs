using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Staff;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Staff.Queries;

/// <summary>
/// Davet listesi sorgusu handler'ı.
/// </summary>
public sealed class ListInvitationsQueryHandler : IRequestHandler<ListInvitationsQuery, List<InvitationDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// ListInvitationsQueryHandler constructor.
    /// </summary>
    public ListInvitationsQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<List<InvitationDto>> Handle(ListInvitationsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar davet listesini görebilir.");
        }

        var query = _dbContext.UserInvitations
            .Include(inv => inv.InvitedBy)
            .Where(inv => inv.TenantId == tenantId && !inv.IsDeleted);

        if (request.PendingOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(inv => inv.AcceptedAt == null && inv.ExpiresAt > now);
        }

        var invitations = await query
            .OrderByDescending(inv => inv.CreatedAt)
            .Select(inv => new InvitationDto
            {
                Id = inv.Id,
                Email = inv.Email,
                Role = inv.Role,
                InvitedBy = inv.InvitedBy != null
                    ? $"{inv.InvitedBy.FirstName} {inv.InvitedBy.LastName}".Trim()
                    : "Unknown",
                InvitedAt = inv.CreatedAt,
                ExpiresAt = inv.ExpiresAt,
                IsAccepted = inv.AcceptedAt.HasValue,
                AcceptedAt = inv.AcceptedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return invitations;
    }
}
