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
/// Kural güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateRuleCommandHandler : IRequestHandler<UpdateRuleCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateRuleCommandHandler> _logger;

    public UpdateRuleCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ICacheService cacheService,
        ILogger<UpdateRuleCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var dto = request.Dto;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kural güncelleyebilir.");
        }

        // Rule varlık kontrolü
        var rule = await _dbContext.Rules
            .FirstOrDefaultAsync(r => r.Id == request.RuleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (rule == null)
        {
            throw new NotFoundException("Rule", request.RuleId);
        }

        // Name unique kontrolü (mevcut kural hariç)
        var nameExists = await _dbContext.Rules
            .AnyAsync(r =>
                r.TenantId == tenantId &&
                r.Id != request.RuleId &&
                r.Name.ToLower() == dto.Name.ToLower() &&
                !r.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (nameExists)
        {
            throw new BusinessRuleException(
                $"'{dto.Name}' adında başka bir kural zaten mevcut.",
                "RULE_NAME_EXISTS");
        }

        // JSON version field kontrolü
        ValidateJsonVersionField(dto.ConditionsJson, "ConditionsJson");
        ValidateJsonVersionField(dto.ActionsJson, "ActionsJson");

        // Eski değerleri kaydet (audit için)
        var oldValue = JsonSerializer.Serialize(new
        {
            rule.Name,
            rule.RuleType,
            rule.Priority,
            rule.TriggerType,
            rule.IsActive
        });

        // Güncelle
        rule.Name = dto.Name;
        rule.Description = dto.Description;
        rule.RuleType = dto.RuleType;
        rule.ConditionsJson = dto.ConditionsJson;
        rule.ActionsJson = dto.ActionsJson;
        rule.Priority = dto.Priority;
        rule.TriggerType = dto.TriggerType;
        rule.IsActive = dto.IsActive;
        rule.ApplicableTimeSlots = dto.ApplicableTimeSlots;
        rule.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "RULE_UPDATED",
            EntityType = "Rule",
            EntityId = rule.Id.ToString(),
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(new
            {
                dto.Name,
                dto.RuleType,
                dto.Priority,
                dto.TriggerType,
                dto.IsActive
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await RuleEngineRulesCacheInvalidation
            .InvalidateForTenantAsync(_cacheService, tenantId, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Kural güncellendi: RuleId={RuleId}, Name={Name}",
            rule.Id, rule.Name);

        return Unit.Value;
    }

    /// <summary>
    /// JSON'da version alanı kontrolü.
    /// </summary>
    private static void ValidateJsonVersionField(string json, string fieldName)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("version", out _))
            {
                throw new BusinessRuleException(
                    $"{fieldName} 'version' alanı içermelidir.",
                    "JSON_VERSION_MISSING");
            }
        }
        catch (JsonException)
        {
            throw new BusinessRuleException(
                $"{fieldName} geçerli bir JSON formatında değil.",
                "INVALID_JSON_FORMAT");
        }
    }
}
