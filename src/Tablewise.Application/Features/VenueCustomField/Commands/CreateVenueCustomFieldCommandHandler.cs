using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.VenueCustomField.Commands;

/// <summary>
/// Custom field oluşturma komutu handler'ı.
/// </summary>
public sealed class CreateVenueCustomFieldCommandHandler : IRequestHandler<CreateVenueCustomFieldCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateVenueCustomFieldCommandHandler> _logger;

    public CreateVenueCustomFieldCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<CreateVenueCustomFieldCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateVenueCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar custom field ekleyebilir.");
        }

        // Venue kontrolü
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Label unique kontrolü (venue içinde)
        var labelExists = await _dbContext.VenueCustomFields
            .AnyAsync(cf => 
                cf.VenueId == request.VenueId && 
                cf.Label.ToLower() == request.Label.ToLower() && 
                !cf.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (labelExists)
        {
            throw new BusinessRuleException(
                $"'{request.Label}' adında bir alan zaten mevcut.",
                "CUSTOM_FIELD_LABEL_EXISTS");
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

        // SortOrder otomatik belirleme (maks + 1)
        var maxSortOrder = await _dbContext.VenueCustomFields
            .Where(cf => cf.VenueId == request.VenueId && !cf.IsDeleted)
            .MaxAsync(cf => (int?)cf.SortOrder, cancellationToken)
            .ConfigureAwait(false);

        var sortOrder = (maxSortOrder ?? 0) + 1;

        // Custom field oluştur
        var customField = new Domain.Entities.VenueCustomField
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VenueId = request.VenueId,
            Label = request.Label,
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            SortOrder = sortOrder,
            Options = request.Options,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.VenueCustomFields.Add(customField);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "CUSTOM_FIELD_CREATED",
            EntityType = "VenueCustomField",
            EntityId = customField.Id.ToString(),
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

        _logger.LogInformation(
            "Custom field oluşturuldu: VenueId={VenueId}, Label={Label}",
            request.VenueId, request.Label);

        return customField.Id;
    }
}
