using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Features.Rule.Commands;
using Tablewise.Application.Interfaces;
using Tablewise.Application.RuleEngine;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural oluşturma komutu handler'ı.
/// </summary>
public sealed class CreateRuleCommandHandler : IRequestHandler<CreateRuleCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPlanLimitService _planLimitService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateRuleCommandHandler> _logger;

    public CreateRuleCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IPlanLimitService planLimitService,
        ICacheService cacheService,
        ILogger<CreateRuleCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _planLimitService = planLimitService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var dto = request.Dto;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kural ekleyebilir.");
        }

        // Plan limiti kontrolü
        await _planLimitService.CheckRuleLimitAsync(tenantId, cancellationToken)
            .ConfigureAwait(false);

        // VenueId varsa venue kontrolü
        if (dto.VenueId.HasValue)
        {
            var venueExists = await _dbContext.Venues
                .AnyAsync(v => v.Id == dto.VenueId.Value && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
                .ConfigureAwait(false);

            if (!venueExists)
            {
                throw new NotFoundException("Venue", dto.VenueId.Value);
            }
        }

        // Name unique kontrolü (tenant içinde)
        var nameExists = await _dbContext.Rules
            .AnyAsync(r =>
                r.TenantId == tenantId &&
                r.Name.ToLower() == dto.Name.ToLower() &&
                !r.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (nameExists)
        {
            throw new BusinessRuleException(
                $"'{dto.Name}' adında bir kural zaten mevcut.",
                "RULE_NAME_EXISTS");
        }

        // JSON version field kontrolü
        ValidateJsonVersionField(dto.ConditionsJson, "ConditionsJson");
        ValidateJsonVersionField(dto.ActionsJson, "ActionsJson");

        // Kural oluştur
        var rule = new Domain.Entities.Rule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = dto.VenueId,
            Name = dto.Name,
            Description = dto.Description,
            RuleType = dto.RuleType,
            ConditionsJson = dto.ConditionsJson,
            ActionsJson = dto.ActionsJson,
            Priority = dto.Priority,
            TriggerType = dto.TriggerType,
            IsActive = dto.IsActive,
            ApplicableTimeSlots = dto.ApplicableTimeSlots,
            TimesTriggered = 0,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Rules.Add(rule);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "RULE_CREATED",
            EntityType = "Rule",
            EntityId = rule.Id.ToString(),
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
            "Kural oluşturuldu: RuleId={RuleId}, Name={Name}, Type={Type}",
            rule.Id, rule.Name, rule.RuleType);

        return rule.Id;
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
