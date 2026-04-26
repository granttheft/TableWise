using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue oluşturma komutu handler'ı.
/// </summary>
public sealed class CreateVenueCommandHandler : IRequestHandler<CreateVenueCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPlanLimitService _planLimitService;
    private readonly ILogger<CreateVenueCommandHandler> _logger;

    public CreateVenueCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IPlanLimitService planLimitService,
        ILogger<CreateVenueCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _planLimitService = planLimitService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner oluşturabilir
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar mekan oluşturabilir.");
        }

        // Plan limitlerini kontrol et
        var tenant = await _dbContext.Tenants
            .Include(t => t.Plan)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new NotFoundException("Tenant", tenantId);
        }

        var currentVenueCount = await _dbContext.Venues
            .CountAsync(v => v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        var venueLimit = _planLimitService.GetVenueLimit(tenant.Plan!.Tier);

        if (currentVenueCount >= venueLimit)
        {
            throw new BusinessRuleException(
                $"Plan limitiniz doldu. Mevcut planınızda en fazla {venueLimit} mekan oluşturabilirsiniz. Planınızı yükseltin.",
                "VENUE_LIMIT_REACHED");
        }

        // Kapora modülü için plan kontrolü (Pro+ gerekli)
        if (request.DepositEnabled && tenant.Plan.Tier < PlanTier.Pro)
        {
            throw new BusinessRuleException(
                "Kapora modülü Pro veya daha üst plan gerektirir.",
                "DEPOSIT_REQUIRES_PRO_PLAN");
        }

        // Venue oluştur
        var venue = new Domain.Entities.Venue
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            Description = request.Description,
            TimeZone = request.TimeZone,
            SlotDurationMinutes = request.SlotDurationMinutes,
            DepositEnabled = request.DepositEnabled,
            DepositAmount = request.DepositAmount,
            DepositPerPerson = request.DepositPerPerson,
            DepositRefundPolicy = request.DepositRefundPolicy,
            DepositRefundHours = request.DepositRefundHours,
            DepositPartialPercent = request.DepositPartialPercent,
            WorkingHours = request.WorkingHours,
            OpeningTime = TimeSpan.FromHours(10), // Varsayılan: 10:00
            ClosingTime = TimeSpan.FromHours(22),  // Varsayılan: 22:00
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Venues.Add(venue);

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "VENUE_CREATED",
            EntityType = "Venue",
            EntityId = venue.Id.ToString(),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new { venue.Name, venue.Address }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Yeni venue oluşturuldu: VenueId={VenueId}, Name={Name}", venue.Id, venue.Name);

        return venue.Id;
    }
}
