using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.VenueClosure.Commands;

/// <summary>
/// Venue kapalılık silme komutu handler'ı.
/// </summary>
public sealed class DeleteVenueClosureCommandHandler : IRequestHandler<DeleteVenueClosureCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteVenueClosureCommandHandler> _logger;

    public DeleteVenueClosureCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<DeleteVenueClosureCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteVenueClosureCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kapalılık silebilir.");
        }

        // Kapalılık kaydını bul
        var closure = await _dbContext.VenueClosures
            .FirstOrDefaultAsync(vc => 
                vc.Id == request.ClosureId && 
                vc.VenueId == request.VenueId && 
                vc.TenantId == tenantId && 
                !vc.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (closure == null)
        {
            throw new NotFoundException("VenueClosure", request.ClosureId);
        }

        // Soft delete
        closure.IsDeleted = true;
        closure.DeletedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "VENUE_CLOSURE_DELETED",
            EntityType = "VenueClosure",
            EntityId = closure.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                closure.Date,
                closure.Reason
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Venue kapalılık silindi: ClosureId={ClosureId}", closure.Id);

        return Unit.Value;
    }
}
