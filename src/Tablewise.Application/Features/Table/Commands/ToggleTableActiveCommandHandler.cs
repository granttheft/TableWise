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
/// Masa aktiflik durumu toggle komutu handler'ı.
/// </summary>
public sealed class ToggleTableActiveCommandHandler : IRequestHandler<ToggleTableActiveCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ToggleTableActiveCommandHandler> _logger;

    public ToggleTableActiveCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<ToggleTableActiveCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(ToggleTableActiveCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar masa durumunu değiştirebilir.");
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

        // Aktiften pasife çeviriyorsak, aktif rezervasyon kontrolü (uyarı)
        if (table.IsActive)
        {
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
                _logger.LogWarning(
                    "Aktif rezervasyonu olan masa deaktive ediliyor: TableId={TableId}",
                    request.TableId);
                
                throw new BusinessRuleException(
                    "Bu masanın aktif rezervasyonları var. Masa deaktive edilemez.",
                    "TABLE_HAS_ACTIVE_RESERVATIONS");
            }
        }

        // Toggle
        var oldStatus = table.IsActive;
        table.IsActive = !table.IsActive;
        table.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "TABLE_TOGGLED",
            EntityType = "Table",
            EntityId = table.Id.ToString(),
            OldValue = oldStatus.ToString(),
            NewValue = table.IsActive.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Masa aktiflik durumu değiştirildi: TableId={TableId}, OldStatus={OldStatus}, NewStatus={NewStatus}",
            table.Id, oldStatus, table.IsActive);

        return Unit.Value;
    }
}
