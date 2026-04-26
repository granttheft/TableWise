using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Tenant güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateTenantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner güncelleyebilir
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar tenant bilgilerini güncelleyebilir.");
        }

        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new NotFoundException("Tenant", tenantId);
        }

        // Eski değerleri kaydet (audit için)
        var oldName = tenant.Name;
        var oldSettings = tenant.Settings;

        // Güncelle
        tenant.Name = request.Name;
        tenant.Settings = request.Settings;
        tenant.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "TENANT_UPDATED",
            EntityType = "Tenant",
            EntityId = tenantId.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(new { Name = oldName, Settings = oldSettings }),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new { request.Name, request.Settings }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Tenant güncellendi: TenantId={TenantId}", tenantId);

        return Unit.Value;
    }
}
