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
/// Kural sıralama güncelleme komutu handler'ı.
/// </summary>
public sealed class ReorderRulesCommandHandler : IRequestHandler<ReorderRulesCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ReorderRulesCommandHandler> _logger;

    public ReorderRulesCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<ReorderRulesCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(ReorderRulesCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var dto = request.Dto;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kural sıralaması değiştirebilir.");
        }

        if (dto.Rules == null || dto.Rules.Count == 0)
        {
            throw new BusinessRuleException("En az bir kural sıralaması belirtilmelidir.", "EMPTY_REORDER_LIST");
        }

        // Tüm rule ID'leri al ve kontrol et
        var ruleIds = dto.Rules.Select(r => r.Id).ToList();
        var rules = await _dbContext.Rules
            .Where(r => ruleIds.Contains(r.Id) && r.TenantId == tenantId && !r.IsDeleted)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rules.Count != ruleIds.Count)
        {
            throw new BusinessRuleException("Bazı kurallar bulunamadı veya silinmiş.", "RULE_NOT_FOUND");
        }

        // Öncelikleri güncelle
        foreach (var item in dto.Rules)
        {
            var rule = rules.First(r => r.Id == item.Id);
            rule.Priority = item.Priority;
            rule.UpdatedAt = DateTime.UtcNow;
        }

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "RULES_REORDERED",
            EntityType = "Rule",
            EntityId = "Bulk",
            NewValue = JsonSerializer.Serialize(dto.Rules.Select(r => new { r.Id, r.Priority })),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Kural sıralaması güncellendi: RuleCount={Count}",
            rules.Count);

        return Unit.Value;
    }
}
