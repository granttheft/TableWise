using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue silme komutu handler'ı (soft delete).
/// </summary>
public sealed class DeleteVenueCommandHandler : IRequestHandler<DeleteVenueCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteVenueCommandHandler> _logger;

    public DeleteVenueCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<DeleteVenueCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteVenueCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner silebilir
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar venue silebilir.");
        }

        // Venue'yü bul
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Aktif rezervasyon kontrolü
        var hasActiveReservations = await _dbContext.Reservations
            .AnyAsync(r => 
                r.VenueId == request.VenueId && 
                !r.IsDeleted && 
                r.ReservedFor >= DateTime.UtcNow.Date &&
                r.Status != ReservationStatus.Cancelled,
                cancellationToken)
            .ConfigureAwait(false);

        if (hasActiveReservations)
        {
            throw new BusinessRuleException(
                "Bu mekanın aktif rezervasyonları var. Önce tüm rezervasyonları iptal etmelisiniz.",
                "VENUE_HAS_ACTIVE_RESERVATIONS");
        }

        // Soft delete
        venue.IsDeleted = true;
        venue.DeletedAt = DateTime.UtcNow;

        // İlişkili masaları da soft delete yap
        var tables = await _dbContext.Tables
            .Where(t => t.VenueId == request.VenueId && !t.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var table in tables)
        {
            table.IsDeleted = true;
            table.DeletedAt = DateTime.UtcNow;
        }

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "VENUE_DELETED",
            EntityType = "Venue",
            EntityId = venue.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(new { venue.Name, venue.Address }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Venue silindi: VenueId={VenueId}, Name={Name}", venue.Id, venue.Name);

        return Unit.Value;
    }
}
