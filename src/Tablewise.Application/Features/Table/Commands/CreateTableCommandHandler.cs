using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Table.Commands;

/// <summary>
/// Masa oluşturma komutu handler'ı.
/// </summary>
public sealed class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPlanLimitService _planLimitService;
    private readonly ILogger<CreateTableCommandHandler> _logger;

    public CreateTableCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IPlanLimitService planLimitService,
        ILogger<CreateTableCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _planLimitService = planLimitService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateTableCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar masa ekleyebilir.");
        }

        // Venue kontrolü
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Plan limiti kontrolü
        await _planLimitService.CheckTableLimitAsync(tenantId, request.VenueId, cancellationToken)
            .ConfigureAwait(false);

        // Name unique kontrolü (venue içinde)
        var nameExists = await _dbContext.Tables
            .AnyAsync(t => 
                t.VenueId == request.VenueId && 
                t.Name.ToLower() == request.Name.ToLower() && 
                !t.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (nameExists)
        {
            throw new BusinessRuleException(
                $"'{request.Name}' adında bir masa zaten mevcut.",
                "TABLE_NAME_EXISTS");
        }

        // SortOrder otomatik belirleme (maks + 1)
        var maxSortOrder = await _dbContext.Tables
            .Where(t => t.VenueId == request.VenueId && !t.IsDeleted)
            .MaxAsync(t => (int?)t.SortOrder, cancellationToken)
            .ConfigureAwait(false);

        var sortOrder = (maxSortOrder ?? 0) + 1;

        // Masa oluştur
        var table = new Domain.Entities.Table
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = request.VenueId,
            Name = request.Name,
            Capacity = request.Capacity,
            Location = request.Location,
            Description = request.Description,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Tables.Add(table);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "TABLE_CREATED",
            EntityType = "Table",
            EntityId = table.Id.ToString(),
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

        _logger.LogInformation(
            "Masa oluşturuldu: VenueId={VenueId}, TableId={TableId}, Name={Name}",
            request.VenueId, table.Id, request.Name);

        return table.Id;
    }
}
