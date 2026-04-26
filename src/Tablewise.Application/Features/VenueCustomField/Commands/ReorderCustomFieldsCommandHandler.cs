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
/// Custom field sıralama güncelleme komutu handler'ı.
/// </summary>
public sealed class ReorderCustomFieldsCommandHandler : IRequestHandler<ReorderCustomFieldsCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ReorderCustomFieldsCommandHandler> _logger;

    public ReorderCustomFieldsCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<ReorderCustomFieldsCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(ReorderCustomFieldsCommand request, CancellationToken cancellationToken)
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

        // Custom field'ları bul
        var customFieldIds = request.Orders.Select(o => o.Id).ToList();
        var customFields = await _dbContext.VenueCustomFields
            .Where(cf => 
                customFieldIds.Contains(cf.Id) && 
                cf.VenueId == request.VenueId && 
                cf.TenantId == tenantId && 
                !cf.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (customFields.Count != request.Orders.Count)
        {
            throw new BusinessRuleException(
                "Bazı custom field'lar bulunamadı.",
                "CUSTOM_FIELDS_NOT_FOUND");
        }

        // Sıralamaları güncelle
        foreach (var order in request.Orders)
        {
            var customField = customFields.First(cf => cf.Id == order.Id);
            customField.SortOrder = order.SortOrder;
            customField.UpdatedAt = DateTime.UtcNow;
        }

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "CUSTOM_FIELDS_REORDERED",
            EntityType = "VenueCustomField",
            EntityId = request.VenueId.ToString(),
            NewValue = System.Text.Json.JsonSerializer.Serialize(request.Orders),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Custom field sıralaması güncellendi: VenueId={VenueId}, Count={Count}",
            request.VenueId, request.Orders.Count);

        return Unit.Value;
    }
}
