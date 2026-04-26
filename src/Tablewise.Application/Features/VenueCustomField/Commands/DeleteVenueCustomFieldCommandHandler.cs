using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.VenueCustomField.Commands;

/// <summary>
/// Custom field silme komutu handler'ı.
/// </summary>
public sealed class DeleteVenueCustomFieldCommandHandler : IRequestHandler<DeleteVenueCustomFieldCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteVenueCustomFieldCommandHandler> _logger;

    public DeleteVenueCustomFieldCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<DeleteVenueCustomFieldCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteVenueCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar custom field silebilir.");
        }

        // Custom field bul
        var customField = await _dbContext.VenueCustomFields
            .FirstOrDefaultAsync(cf => 
                cf.Id == request.CustomFieldId && 
                cf.VenueId == request.VenueId && 
                cf.TenantId == tenantId && 
                !cf.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (customField == null)
        {
            throw new NotFoundException("VenueCustomField", request.CustomFieldId);
        }

        // Soft delete
        customField.IsDeleted = true;
        customField.DeletedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "CUSTOM_FIELD_DELETED",
            EntityType = "VenueCustomField",
            EntityId = customField.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                customField.Label,
                customField.FieldType
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Custom field silindi: CustomFieldId={CustomFieldId}", customField.Id);

        return Unit.Value;
    }
}
