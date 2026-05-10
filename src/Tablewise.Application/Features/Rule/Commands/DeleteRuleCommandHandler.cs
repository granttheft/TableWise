using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural silme komutu handler'ı.
/// </summary>
public sealed class DeleteRuleCommandHandler : IRequestHandler<DeleteRuleCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteRuleCommandHandler> _logger;

    public DeleteRuleCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<DeleteRuleCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kural silebilir.");
        }

        // Rule varlık kontrolü
        var rule = await _dbContext.Rules
            .FirstOrDefaultAsync(r => r.Id == request.RuleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (rule == null)
        {
            throw new NotFoundException("Rule", request.RuleId);
        }

        // Soft delete
        rule.IsDeleted = true;
        rule.DeletedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "RULE_DELETED",
            EntityType = "Rule",
            EntityId = rule.Id.ToString(),
            OldValue = JsonSerializer.Serialize(new
            {
                rule.Name,
                rule.RuleType
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Kural silindi: RuleId={RuleId}, Name={Name}",
            rule.Id, rule.Name);

        return Unit.Value;
    }
}
