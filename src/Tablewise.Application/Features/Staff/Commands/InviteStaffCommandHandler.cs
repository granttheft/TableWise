using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Auth;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Personel davet komutu handler'ı.
/// </summary>
public sealed class InviteStaffCommandHandler : IRequestHandler<InviteStaffCommand, Guid>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailService _emailService;
    private readonly AuthSettings _authSettings;
    private readonly ILogger<InviteStaffCommandHandler> _logger;

    /// <summary>
    /// InviteStaffCommandHandler constructor.
    /// </summary>
    public InviteStaffCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IEmailService emailService,
        IOptions<AuthSettings> authSettings,
        ILogger<InviteStaffCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _emailService = emailService;
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var currentUserId = _currentUser.UserId;

        // Yetki kontrolü - sadece Owner davet gönderebilir
        var currentUserRole = _currentUser.Role;
        if (currentUserRole != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar personel daveti gönderebilir.");
        }

        var emailLower = request.Email.ToLowerInvariant();

        // Email zaten bu tenant'ta kullanıcı mı?
        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.TenantId == tenantId && u.Email.ToLower() == emailLower && !u.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (existingUser)
        {
            throw new BusinessRuleException(
                "Bu email adresi zaten kayıtlı.",
                "EMAIL_ALREADY_EXISTS");
        }

        // Aktif davet var mı kontrol et
        var activeInvitation = await _dbContext.UserInvitations
            .AnyAsync(inv =>
                inv.TenantId == tenantId &&
                inv.Email.ToLower() == emailLower &&
                !inv.IsDeleted &&
                inv.ExpiresAt > DateTime.UtcNow &&
                inv.AcceptedAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        if (activeInvitation)
        {
            throw new BusinessRuleException(
                "Bu email adresine zaten aktif bir davet gönderilmiş. Lütfen mevcut davetin süresinin dolmasını bekleyin veya iptal edin.",
                "ACTIVE_INVITATION_EXISTS");
        }

        // Davet eden kullanıcı bilgisini al
        var inviter = await _dbContext.Users
            .Where(u => u.Id == currentUserId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (inviter == null)
        {
            throw new UnauthorizedException("Kullanıcı bilgisi bulunamadı.");
        }

        // Tenant bilgisini al
        var tenant = await _dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => new { t.Name })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new NotFoundException("Tenant", tenantId);
        }

        // UserInvitation oluştur
        var invitation = new UserInvitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = emailLower,
            Role = request.Role,
            Token = Guid.NewGuid().ToString("N"), // 32 karakter hex
            InvitedBy = currentUserId,
            InvitedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserInvitations.Add(invitation);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = currentUserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "STAFF_INVITED",
            EntityType = "UserInvitation",
            EntityId = invitation.Id.ToString(),
            NewValue = $"{{\"email\":\"{emailLower}\",\"role\":\"{request.Role}\"}}",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Email gönder (fire and forget - hata yutulur)
        _ = SendInvitationEmailSafeAsync(
            invitation.Email,
            tenant.Name,
            $"{inviter.FirstName} {inviter.LastName}".Trim(),
            invitation.Role.ToString(),
            invitation.Token);

        _logger.LogInformation(
            "Personel daveti gönderildi: Email={Email}, Role={Role}, TenantId={TenantId}",
            "***", request.Role, tenantId);

        return invitation.Id;
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
