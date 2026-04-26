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
/// Masa sıralama güncelleme komutu handler'ı.
/// </summary>
public sealed class ReorderTablesCommandHandler : IRequestHandler<ReorderTablesCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ReorderTablesCommandHandler> _logger;

    public ReorderTablesCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<ReorderTablesCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(ReorderTablesCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar sıralama güncelleyebilir.");
        }

        // Venue kontrolü
        var venueExists = await _dbContext.Venues
            .AnyAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!venueExists)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Maksimum 100 item kontrolü
        if (request.Orders.Count > 100)
        {
            throw new BusinessRuleException(
                "Tek seferde maksimum 100 masa sıralanabilir.",
                "TOO_MANY_TABLES");
        }

        // Masaları bul
        var tableIds = request.Orders.Select(o => o.Id).ToList();
        var tables = await _dbContext.Tables
            .Where(t => 
                tableIds.Contains(t.Id) && 
                t.VenueId == request.VenueId && 
                t.TenantId == tenantId && 
                !t.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tables.Count != request.Orders.Count)
        {
            throw new BusinessRuleException(
                "Bazı masalar bulunamadı.",
                "TABLES_NOT_FOUND");
        }

        // Sıralamaları güncelle
        foreach (var order in request.Orders)
        {
            var table = tables.First(t => t.Id == order.Id);
            table.SortOrder = order.SortOrder;
            table.UpdatedAt = DateTime.UtcNow;
        }

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "TABLES_REORDERED",
            EntityType = "Table",
            EntityId = request.VenueId.ToString(),
            NewValue = System.Text.Json.JsonSerializer.Serialize(request.Orders),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Masa sıralaması güncellendi: VenueId={VenueId}, Count={Count}",
            request.VenueId, request.Orders.Count);

        return Unit.Value;
    }
}
