using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.Table.Commands;

/// <summary>
/// Masa güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateTableCommandHandler : IRequestHandler<UpdateTableCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateTableCommandHandler> _logger;

    public UpdateTableCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateTableCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateTableCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar masa güncelleyebilir.");
        }

        // Masa bul
        var table = await _dbContext.Tables
            .FirstOrDefaultAsync(t => 
                t.Id == request.TableId && 
                t.VenueId == request.VenueId && 
                t.TenantId == tenantId && 
                !t.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
        {
            throw new NotFoundException("Table", request.TableId);
        }

        // Name değişiyorsa unique kontrolü
        if (table.Name.ToLower() != request.Name.ToLower())
        {
            var nameExists = await _dbContext.Tables
                .AnyAsync(t => 
                    t.VenueId == request.VenueId && 
                    t.Name.ToLower() == request.Name.ToLower() && 
                    t.Id != request.TableId &&
                    !t.IsDeleted,
                    cancellationToken)
                .ConfigureAwait(false);

            if (nameExists)
            {
                throw new BusinessRuleException(
                    $"'{request.Name}' adında bir masa zaten mevcut.",
                    "TABLE_NAME_EXISTS");
            }
        }

        // Eski değerleri kaydet
        var oldValues = new
        {
            table.Name,
            table.Capacity,
            table.Location
        };

        // Güncelle
        table.Name = request.Name;
        table.Capacity = request.Capacity;
        table.Location = request.Location;
        table.Description = request.Description;
        table.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "TABLE_UPDATED",
            EntityType = "Table",
            EntityId = table.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Name,
                request.Capacity,
                request.Location
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Masa güncellendi: TableId={TableId}", table.Id);

        return Unit.Value;
    }
}
