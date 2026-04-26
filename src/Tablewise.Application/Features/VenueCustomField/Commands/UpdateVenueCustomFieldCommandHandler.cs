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
/// Custom field güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateVenueCustomFieldCommandHandler : IRequestHandler<UpdateVenueCustomFieldCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateVenueCustomFieldCommandHandler> _logger;

    public UpdateVenueCustomFieldCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateVenueCustomFieldCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateVenueCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar custom field güncelleyebilir.");
        }

        // Custom field bul
        var customField = await _dbContext.VenueCustomFields
            .FirstOrDefaultAsync(cf => 
                cf.Id == request.CustomFieldId && 
                cf.VenueId == request.VenueId && 
                cf.TenantId == tenantId && 
                !cf.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (customField == null)
        {
            throw new NotFoundException("VenueCustomField", request.CustomFieldId);
        }

        // Label değişiyorsa unique kontrolü
        if (customField.Label.ToLower() != request.Label.ToLower())
        {
            var labelExists = await _dbContext.VenueCustomFields
                .AnyAsync(cf => 
                    cf.VenueId == request.VenueId && 
                    cf.Label.ToLower() == request.Label.ToLower() && 
                    cf.Id != request.CustomFieldId &&
                    !cf.IsDeleted,
                    cancellationToken)
                .ConfigureAwait(false);

            if (labelExists)
            {
                throw new BusinessRuleException(
                    $"'{request.Label}' adında bir alan zaten mevcut.",
                    "CUSTOM_FIELD_LABEL_EXISTS");
            }
        }

        // Select tipi için options kontrolü
        if (request.FieldType == CustomFieldType.Select)
        {
            if (string.IsNullOrEmpty(request.Options))
            {
                throw new BusinessRuleException(
                    "Select tipi için seçenekler zorunludur.",
                    "SELECT_REQUIRES_OPTIONS");
            }

            // JSON validasyonu
            try
            {
                System.Text.Json.JsonDocument.Parse(request.Options);
            }
            catch
            {
                throw new BusinessRuleException(
                    "Seçenekler geçerli bir JSON array formatında olmalıdır.",
                    "INVALID_OPTIONS_FORMAT");
            }
        }

        // Eski değerleri kaydet
        var oldValues = new
        {
            customField.Label,
            customField.FieldType,
            customField.IsRequired
        };

        // Güncelle
        customField.Label = request.Label;
        customField.FieldType = request.FieldType;
        customField.IsRequired = request.IsRequired;
        customField.Options = request.Options;
        customField.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "CUSTOM_FIELD_UPDATED",
            EntityType = "VenueCustomField",
            EntityId = customField.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Label,
                request.FieldType,
                request.IsRequired
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Custom field güncellendi: CustomFieldId={CustomFieldId}", customField.Id);

        return Unit.Value;
    }
}
