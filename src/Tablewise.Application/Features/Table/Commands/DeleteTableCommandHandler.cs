using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Table.Commands;

/// <summary>
/// Masa silme komutu handler'ı (soft delete).
/// </summary>
public sealed class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteTableCommandHandler> _logger;

    public DeleteTableCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<DeleteTableCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar masa silebilir.");
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

        // Aktif rezervasyon kontrolü
        var hasActiveReservations = await _dbContext.Reservations
            .AnyAsync(r => 
                r.TableId == request.TableId && 
                !r.IsDeleted &&
                r.ReservedFor >= DateTime.UtcNow.Date &&
                r.Status != ReservationStatus.Cancelled,
                cancellationToken)
            .ConfigureAwait(false);

        if (hasActiveReservations)
        {
            throw new BusinessRuleException(
                "Bu masanın aktif rezervasyonları var. Masa silinemez.",
                "TABLE_HAS_ACTIVE_RESERVATIONS");
        }

        // Soft delete
        table.IsDeleted = true;
        table.DeletedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "TABLE_DELETED",
            EntityType = "Table",
            EntityId = table.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                table.Name,
                table.Capacity
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Masa silindi: TableId={TableId}", table.Id);

        return Unit.Value;
    }
}
