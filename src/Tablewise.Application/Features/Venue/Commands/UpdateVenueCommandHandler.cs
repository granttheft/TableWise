using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue güncelleme komutu handler'ı.
/// </summary>
public sealed class UpdateVenueCommandHandler : IRequestHandler<UpdateVenueCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateVenueCommandHandler> _logger;

    public UpdateVenueCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<UpdateVenueCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateVenueCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner güncelleyebilir
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar venue güncelleyebilir.");
        }

        // Venue'yü bul
        var venue = await _dbContext.Venues
            .Include(v => v.Tenant)
            .ThenInclude(t => t!.Plan)
            .FirstOrDefaultAsync(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.VenueId);
        }

        // Kapora modülü için plan kontrolü (Pro+ gerekli)
        if (request.DepositEnabled && venue.Tenant!.Plan!.Tier < PlanTier.Pro)
        {
            throw new BusinessRuleException(
                "Kapora modülü Pro veya daha üst plan gerektirir.",
                "DEPOSIT_REQUIRES_PRO_PLAN");
        }

        // Eski değerleri kaydet (audit için)
        var oldValues = new
        {
            venue.Name,
            venue.Address,
            venue.DepositEnabled,
            venue.DepositAmount
        };

        // Güncelle
        venue.Name = request.Name;
        venue.Address = request.Address;
        venue.PhoneNumber = request.PhoneNumber;
        venue.Description = request.Description;
        venue.TimeZone = request.TimeZone;
        venue.SlotDurationMinutes = request.SlotDurationMinutes;
        venue.DepositEnabled = request.DepositEnabled;
        venue.DepositAmount = request.DepositAmount;
        venue.DepositPerPerson = request.DepositPerPerson;
        venue.DepositRefundPolicy = request.DepositRefundPolicy;
        venue.DepositRefundHours = request.DepositRefundHours;
        venue.DepositPartialPercent = request.DepositPartialPercent;
        venue.WorkingHours = request.WorkingHours;
        venue.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "VENUE_UPDATED",
            EntityType = "Venue",
            EntityId = venue.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Name,
                request.Address,
                request.DepositEnabled,
                request.DepositAmount
            }),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Venue güncellendi: VenueId={VenueId}, Name={Name}", venue.Id, venue.Name);

        return Unit.Value;
    }
}
