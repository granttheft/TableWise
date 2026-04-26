using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence;

namespace Tablewise.Application.Features.VenueClosure.Commands;

/// <summary>
/// Venue kapalılık oluşturma komutu handler'ı.
/// </summary>
public sealed class CreateVenueClosureCommandHandler : IRequestHandler<CreateVenueClosureCommand, List<Guid>>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateVenueClosureCommandHandler> _logger;

    public CreateVenueClosureCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<CreateVenueClosureCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<Guid>> Handle(CreateVenueClosureCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kapalılık ekleyebilir.");
        }

        // Venue kontrolü
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // StartDate < EndDate kontrolü
        if (request.StartDate > request.EndDate)
        {
            throw new BusinessRuleException(
                "Başlangıç tarihi bitiş tarihinden büyük olamaz.",
                "INVALID_DATE_RANGE");
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

        var createdIds = new List<Guid>();
        var currentDate = request.StartDate.Date;

        // Her gün için kapalılık kaydı oluştur
        while (currentDate <= request.EndDate.Date)
        {
            // Çakışma kontrolü
            var hasOverlap = await _dbContext.VenueClosures
                .AnyAsync(vc => 
                    vc.VenueId == request.VenueId && 
                    vc.Date == currentDate && 
                    !vc.IsDeleted,
                    cancellationToken)
                .ConfigureAwait(false);

            if (hasOverlap)
            {
                _logger.LogWarning(
                    "Kapalılık çakışması: VenueId={VenueId}, Date={Date}",
                    request.VenueId, currentDate);
                
                currentDate = currentDate.AddDays(1);
                continue; // Skip this date
            }

            // Aktif rezervasyon kontrolü (uyarı)
            var hasActiveReservations = await _dbContext.Reservations
                .AnyAsync(r => 
                    r.VenueId == request.VenueId && 
                    r.ReservationDate == currentDate &&
                    !r.IsDeleted &&
                    r.Status != ReservationStatus.Cancelled,
                    cancellationToken)
                .ConfigureAwait(false);

            if (hasActiveReservations)
            {
                _logger.LogWarning(
                    "Bu tarihte aktif rezervasyon var: VenueId={VenueId}, Date={Date}",
                    request.VenueId, currentDate);
            }

            var closure = new Domain.Entities.VenueClosure
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VenueId = request.VenueId,
                Date = currentDate,
                IsFullDay = request.IsFullDay,
                OpenTime = request.OpenTime,
                CloseTime = request.CloseTime,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.VenueClosures.Add(closure);
            createdIds.Add(closure.Id);

            currentDate = currentDate.AddDays(1);
        }

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "VENUE_CLOSURE_CREATED",
            EntityType = "VenueClosure",
            EntityId = request.VenueId.ToString(),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.StartDate,
                request.EndDate,
                Count = createdIds.Count
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Venue kapalılık oluşturuldu: VenueId={VenueId}, Count={Count}",
            request.VenueId, createdIds.Count);

        return createdIds;
    }
}
