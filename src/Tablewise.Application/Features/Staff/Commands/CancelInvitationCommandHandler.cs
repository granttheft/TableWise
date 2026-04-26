using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Davet iptal komutu handler'ı.
/// </summary>
public sealed class CancelInvitationCommandHandler : IRequestHandler<CancelInvitationCommand>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CancelInvitationCommandHandler> _logger;

    /// <summary>
    /// CancelInvitationCommandHandler constructor.
    /// </summary>
    public CancelInvitationCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<CancelInvitationCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(CancelInvitationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar davet iptal edebilir.");
        }

        // Davet bul
        var invitation = await _dbContext.UserInvitations
            .FirstOrDefaultAsync(inv =>
                inv.Id == request.InvitationId &&
                inv.TenantId == tenantId &&
                !inv.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (invitation == null)
        {
            throw new NotFoundException("UserInvitation", request.InvitationId);
        }

        // Zaten kabul edilmiş mi?
        if (invitation.AcceptedAt.HasValue)
        {
            throw new BusinessRuleException(
                "Kabul edilmiş davet iptal edilemez.",
                "INVITATION_ALREADY_ACCEPTED");
        }

        // Soft delete
        invitation.IsDeleted = true;
        invitation.DeletedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "INVITATION_CANCELLED",
            EntityType = "UserInvitation",
            EntityId = invitation.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Davet iptal edildi: InvitationId={InvitationId}, TenantId={TenantId}",
            invitation.Id, tenantId);
    }
}
