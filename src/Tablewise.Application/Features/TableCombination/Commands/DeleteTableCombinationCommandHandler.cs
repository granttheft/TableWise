using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.TableCombination.Commands;

/// <summary>
/// Masa kombinasyonu silme komutu handler'ı (soft delete).
/// </summary>
public sealed class DeleteTableCombinationCommandHandler : IRequestHandler<DeleteTableCombinationCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteTableCombinationCommandHandler> _logger;

    public DeleteTableCombinationCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<DeleteTableCombinationCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteTableCombinationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kombinasyon silebilir.");
        }

        // Kombinasyon bul
        var combination = await _dbContext.TableCombinations
            .FirstOrDefaultAsync(tc => 
                tc.Id == request.CombinationId && 
                tc.VenueId == request.VenueId && 
                tc.TenantId == tenantId && 
                !tc.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (combination == null)
        {
            throw new NotFoundException("TableCombination", request.CombinationId);
        }

        // Soft delete
        combination.IsDeleted = true;
        combination.DeletedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "COMBINATION_DELETED",
            EntityType = "TableCombination",
            EntityId = combination.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                combination.Name,
                combination.CombinedCapacity
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Masa kombinasyonu silindi: CombinationId={CombinationId}", combination.Id);

        return Unit.Value;
    }
}
