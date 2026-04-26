using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue çalışma saatleri güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateWorkingHoursCommandHandler : IRequestHandler<UpdateWorkingHoursCommand, Unit>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateWorkingHoursCommandHandler> _logger;

    public UpdateWorkingHoursCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateWorkingHoursCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateWorkingHoursCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner güncelleyebilir
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar çalışma saatlerini güncelleyebilir.");
        }

        // Venue'yü bul
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // JSON validasyonu
        try
        {
            System.Text.Json.JsonDocument.Parse(request.WorkingHours);
        }
        catch
        {
            throw new BusinessRuleException(
                "Geçersiz JSON formatı. Çalışma saatleri geçerli bir JSON olmalıdır.",
                "INVALID_JSON_FORMAT");
        }

        // Eski değeri kaydet
        var oldWorkingHours = venue.WorkingHours;

        // Güncelle
        venue.WorkingHours = request.WorkingHours;
        venue.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "WORKING_HOURS_UPDATED",
            EntityType = "Venue",
            EntityId = venue.Id.ToString(),
            OldValue = oldWorkingHours,
            NewValue = request.WorkingHours,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Venue çalışma saatleri güncellendi: VenueId={VenueId}", venue.Id);

        return Unit.Value;
    }
}
