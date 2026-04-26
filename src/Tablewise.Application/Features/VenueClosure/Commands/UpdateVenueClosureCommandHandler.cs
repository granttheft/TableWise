using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.VenueClosure.Commands;

/// <summary>
/// Venue kapalılık güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateVenueClosureCommandHandler : IRequestHandler<UpdateVenueClosureCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateVenueClosureCommandHandler> _logger;

    public UpdateVenueClosureCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateVenueClosureCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateVenueClosureCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kapalılık güncelleyebilir.");
        }

        // Kapalılık kaydını bul
        var closure = await _dbContext.VenueClosures
            .FirstOrDefaultAsync(vc => 
                vc.Id == request.ClosureId && 
                vc.VenueId == request.VenueId && 
                vc.TenantId == tenantId && 
                !vc.IsDeleted,
                cancellationToken)
            .ConfigureAwait(false);

        if (closure == null)
        {
            throw new NotFoundException("VenueClosure", request.ClosureId);
        }

        // Kısmi kapalılık için saat kontrolü
        if (!request.IsFullDay)
        {
            if (!request.OpenTime.HasValue || !request.CloseTime.HasValue)
            {
                throw new BusinessRuleException(
                    "Kısmi kapalılık için açılış ve kapanış saatleri zorunludur.",
                    "PARTIAL_CLOSURE_REQUIRES_TIMES");
            }

            if (request.OpenTime >= request.CloseTime)
            {
                throw new BusinessRuleException(
                    "Açılış saati kapanış saatinden küçük olmalıdır.",
                    "INVALID_TIME_RANGE");
            }
        }

        // Tarih değişiyorsa çakışma kontrolü
        if (closure.Date != request.Date)
        {
            var hasOverlap = await _dbContext.VenueClosures
                .AnyAsync(vc => 
                    vc.VenueId == request.VenueId && 
                    vc.Date == request.Date && 
                    vc.Id != request.ClosureId &&
                    !vc.IsDeleted,
                    cancellationToken)
                .ConfigureAwait(false);

            if (hasOverlap)
            {
                throw new BusinessRuleException(
                    "Bu tarihte zaten bir kapalılık kaydı var.",
                    "CLOSURE_ALREADY_EXISTS");
            }
        }

        // Eski değerleri kaydet
        var oldValues = new
        {
            closure.Date,
            closure.IsFullDay,
            closure.Reason
        };

        // Güncelle
        closure.Date = request.Date;
        closure.IsFullDay = request.IsFullDay;
        closure.OpenTime = request.OpenTime;
        closure.CloseTime = request.CloseTime;
        closure.Reason = request.Reason;
        closure.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "VENUE_CLOSURE_UPDATED",
            EntityType = "VenueClosure",
            EntityId = closure.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Date,
                request.IsFullDay,
                request.Reason
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Venue kapalılık güncellendi: ClosureId={ClosureId}", closure.Id);

        return Unit.Value;
    }
}
