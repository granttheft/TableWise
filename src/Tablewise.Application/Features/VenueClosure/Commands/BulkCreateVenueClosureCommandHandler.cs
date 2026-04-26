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
/// Toplu kapalılık oluşturma komutu handler'ı.
/// </summary>
public sealed class BulkCreateVenueClosureCommandHandler : IRequestHandler<BulkCreateVenueClosureCommand, List<Guid>>
{
    private readonly TablewiseDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<BulkCreateVenueClosureCommandHandler> _logger;

    public BulkCreateVenueClosureCommandHandler(
        TablewiseDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<BulkCreateVenueClosureCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<Guid>> Handle(BulkCreateVenueClosureCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar toplu kapalılık ekleyebilir.");
        }

        // Venue kontrolü
        var venue = await _dbContext.Venues
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Maksimum 50 item kontrolü
        if (request.Closures.Count > 50)
        {
            throw new BusinessRuleException(
                "Toplu işlemde maksimum 50 adet kapalılık oluşturabilirsiniz.",
                "BULK_LIMIT_EXCEEDED");
        }

        var createdIds = new List<Guid>();

        // Transaction içinde işle
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var item in request.Closures)
            {
                // Validasyon
                if (item.StartDate > item.EndDate)
                {
                    throw new BusinessRuleException(
                        "Başlangıç tarihi bitiş tarihinden büyük olamaz.",
                        "INVALID_DATE_RANGE");
                }

                if (!item.IsFullDay)
                {
                    if (!item.OpenTime.HasValue || !item.CloseTime.HasValue)
                    {
                        throw new BusinessRuleException(
                            "Kısmi kapalılık için açılış ve kapanış saatleri zorunludur.",
                            "PARTIAL_CLOSURE_REQUIRES_TIMES");
                    }

                    if (item.OpenTime >= item.CloseTime)
                    {
                        throw new BusinessRuleException(
                            "Açılış saati kapanış saatinden küçük olmalıdır.",
                            "INVALID_TIME_RANGE");
                    }
                }

                // Her gün için kayıt oluştur
                var currentDate = item.StartDate.Date;
                while (currentDate <= item.EndDate.Date)
                {
                    // Çakışma kontrolü
                    var hasOverlap = await _dbContext.VenueClosures
                        .AnyAsync(vc => 
                            vc.VenueId == request.VenueId && 
                            vc.Date == currentDate && 
                            !vc.IsDeleted,
                            cancellationToken)
                        .ConfigureAwait(false);

                    if (!hasOverlap)
                    {
                        var closure = new Domain.Entities.VenueClosure
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            VenueId = request.VenueId,
                            Date = currentDate,
                            IsFullDay = item.IsFullDay,
                            OpenTime = item.OpenTime,
                            CloseTime = item.CloseTime,
                            Reason = item.Reason,
                            CreatedAt = DateTime.UtcNow
                        };

                        _dbContext.VenueClosures.Add(closure);
                        createdIds.Add(closure.Id);
                    }

                    currentDate = currentDate.AddDays(1);
                }
            }

            // Audit log
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = _currentUser.UserId,
                PerformedBy = _currentUser.Email ?? "System",
                Action = "VENUE_CLOSURE_BULK_CREATED",
                EntityType = "VenueClosure",
                EntityId = request.VenueId.ToString(),
                NewValue = System.Text.Json.JsonSerializer.Serialize(new
                {
                    ItemCount = request.Closures.Count,
                    CreatedCount = createdIds.Count
                }),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.AuditLogs.Add(auditLog);

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Toplu venue kapalılık oluşturuldu: VenueId={VenueId}, Count={Count}",
                request.VenueId, createdIds.Count);

            return createdIds;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}
