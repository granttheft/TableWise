using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// UpdateVenueWhatsAppSettingsCommand handler'ı.
/// </summary>
public sealed class UpdateVenueWhatsAppSettingsCommandHandler
    : IRequestHandler<UpdateVenueWhatsAppSettingsCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public UpdateVenueWhatsAppSettingsCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(
        UpdateVenueWhatsAppSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var venue = await _dbContext.Venues
            .Where(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
            throw new NotFoundException("Venue", request.VenueId);

        venue.WhatsAppEnabled = request.WhatsAppEnabled;
        venue.WaNotifyReservationReceived = request.NotifyReservationReceived;
        venue.WaNotifyReservationConfirmed = request.NotifyReservationConfirmed;
        venue.WaNotifyReminder = request.NotifyReminder;
        venue.WaNotifyCancellation = request.NotifyCancellation;
        venue.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
