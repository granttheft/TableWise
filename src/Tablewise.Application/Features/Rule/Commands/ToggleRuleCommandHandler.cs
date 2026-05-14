using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Application.RuleEngine;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural aktif/pasif değiştirme komutu handler'ı.
/// </summary>
public sealed class ToggleRuleCommandHandler : IRequestHandler<ToggleRuleCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ToggleRuleCommandHandler> _logger;

    public ToggleRuleCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ICacheService cacheService,
        ILogger<ToggleRuleCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ToggleRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kural durumu değiştirebilir.");
        }

        // Rule varlık kontrolü
        var rule = await _dbContext.Rules
            .FirstOrDefaultAsync(r => r.Id == request.RuleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (rule == null)
        {
            throw new NotFoundException("Rule", request.RuleId);
        }

        // Toggle IsActive
        var oldIsActive = rule.IsActive;
        rule.IsActive = !rule.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "RULE_TOGGLED",
            EntityType = "Rule",
            EntityId = rule.Id.ToString(),
            OldValue = JsonSerializer.Serialize(new { IsActive = oldIsActive }),
            NewValue = JsonSerializer.Serialize(new { IsActive = rule.IsActive }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await RuleEngineRulesCacheInvalidation
            .InvalidateForTenantAsync(_cacheService, tenantId, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Kural durumu değiştirildi: RuleId={RuleId}, Name={Name}, IsActive={IsActive}",
            rule.Id, rule.Name, rule.IsActive);

        return Unit.Value;
    }
}
