using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Settings;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Davet tekrar gönder komutu handler'ı.
/// </summary>
public sealed class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailService _emailService;
    private readonly AuthSettings _authSettings;
    private readonly ILogger<ResendInvitationCommandHandler> _logger;

    /// <summary>
    /// ResendInvitationCommandHandler constructor.
    /// </summary>
    public ResendInvitationCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IEmailService emailService,
        IOptions<AuthSettings> authSettings,
        ILogger<ResendInvitationCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _emailService = emailService;
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar davet tekrar gönderebilir.");
        }

        // Davet bul
        var invitation = await _dbContext.UserInvitations
            .Include(inv => inv.Tenant)
            .Include(inv => inv.InvitedBy)
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
                "Kabul edilmiş davet tekrar gönderilemez.",
                "INVITATION_ALREADY_ACCEPTED");
        }

        // Yeni token ve expiry
        invitation.Token = Guid.NewGuid().ToString("N");
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
        invitation.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "INVITATION_RESENT",
            EntityType = "UserInvitation",
            EntityId = invitation.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Email gönder (fire and forget)
        _ = SendInvitationEmailSafeAsync(
            invitation.Email,
            invitation.Tenant!.Name,
            $"{invitation.InvitedBy!.FirstName} {invitation.InvitedBy.LastName}".Trim(),
            invitation.Role.ToString(),
            invitation.Token);

        _logger.LogInformation(
            "Davet tekrar gönderildi: InvitationId={InvitationId}, TenantId={TenantId}",
            invitation.Id, tenantId);
    }

    /// <summary>
    /// Davet emaili güvenli gönderir (hata yutulur).
    /// </summary>
    private async Task SendInvitationEmailSafeAsync(
        string email,
        string tenantName,
        string inviterName,
        string role,
        string token)
    {
        try
        {
            var inviteLink = $"{_authSettings.AdminPanelUrl}/invite/{token}";
            await _emailService.SendStaffInvitationEmailAsync(
                email,
                tenantName,
                inviterName,
                role,
                inviteLink)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Davet emaili gönderilemedi: Email={Email}", "***");
        }
    }
}
