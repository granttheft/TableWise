using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Personel silme komutu handler'ı.
/// </summary>
public sealed class RemoveStaffCommandHandler : IRequestHandler<RemoveStaffCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<RemoveStaffCommandHandler> _logger;

    /// <summary>
    /// RemoveStaffCommandHandler constructor.
    /// </summary>
    public RemoveStaffCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<RemoveStaffCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(RemoveStaffCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar personel silebilir.");
        }

        // Kullanıcı bul
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Id == request.UserId &&
                u.TenantId == tenantId &&
                !u.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Kendini silme engeli
        if (user.Id == _currentUser.UserId)
        {
            throw new BusinessRuleException(
                "Kendi hesabınızı silemezsiniz.",
                "CANNOT_REMOVE_SELF");
        }

        // Son Owner'ı silme engeli
        if (user.Role == UserRole.Owner)
        {
            var ownerCount = await _dbContext.Users
                .CountAsync(u =>
                    u.TenantId == tenantId &&
                    u.Role == UserRole.Owner &&
                    !u.IsDeleted,
                    cancellationToken)
                .ConfigureAwait(false);

            if (ownerCount <= 1)
            {
                throw new BusinessRuleException(
                    "Son Owner kullanıcısı silinemez. En az bir Owner olmalıdır.",
                    "LAST_OWNER_CANNOT_BE_REMOVED");
            }
        }

        // Soft delete
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;

        // Kullanıcının tüm aktif refresh token'larını revoke et
        var activeTokens = await _dbContext.Set<RevocableRefreshToken>()
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked && !rt.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedBy = "User Removed";
        }

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "STAFF_REMOVED",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            OldValue = $"{{\"email\":\"{user.Email}\",\"role\":\"{user.Role}\"}}",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Kullanıcı silindi: UserId={UserId}, TenantId={TenantId}",
            user.Id, tenantId);
    }
}
