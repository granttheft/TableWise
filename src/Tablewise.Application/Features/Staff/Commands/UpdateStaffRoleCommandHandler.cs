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
/// Personel rol güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateStaffRoleCommandHandler : IRequestHandler<UpdateStaffRoleCommand>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateStaffRoleCommandHandler> _logger;

    /// <summary>
    /// UpdateStaffRoleCommandHandler constructor.
    /// </summary>
    public UpdateStaffRoleCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateStaffRoleCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(UpdateStaffRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar rol değiştirebilir.");
        }

        // SuperAdmin rolüne yükselme engellenir
        if (request.NewRole == UserRole.SuperAdmin)
        {
            throw new ForbiddenException("SuperAdmin rolü atanamaz.");
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

        // Kendini düşürme engeli
        if (user.Id == _currentUser.UserId && request.NewRole != UserRole.Owner)
        {
            throw new BusinessRuleException(
                "Kendi rolünüzü değiştiremezsiniz.",
                "CANNOT_CHANGE_OWN_ROLE");
        }

        // Son Owner'ı Staff'a düşürme engeli
        if (user.Role == UserRole.Owner && request.NewRole != UserRole.Owner)
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
                    "Son Owner kullanıcısının rolü değiştirilemez. En az bir Owner olmalıdır.",
                    "LAST_OWNER_CANNOT_BE_CHANGED");
            }
        }

        var oldRole = user.Role;
        user.Role = request.NewRole;
        user.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "STAFF_ROLE_UPDATED",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            OldValue = $"{{\"role\":\"{oldRole}\"}}",
            NewValue = $"{{\"role\":\"{request.NewRole}\"}}",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Kullanıcı rolü güncellendi: UserId={UserId}, OldRole={OldRole}, NewRole={NewRole}",
            user.Id, oldRole, request.NewRole);
    }
}
