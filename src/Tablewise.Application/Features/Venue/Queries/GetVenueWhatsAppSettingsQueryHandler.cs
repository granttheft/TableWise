using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tablewise.Application.DTOs.Venue;
using Tablewise.Application.Interfaces;
using Tablewise.Application.Settings;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Venue.Queries;

/// <summary>
/// GetVenueWhatsAppSettingsQuery handler'ı.
/// </summary>
public sealed class GetVenueWhatsAppSettingsQueryHandler
    : IRequestHandler<GetVenueWhatsAppSettingsQuery, VenueWhatsAppSettingsDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly WhatsAppSettings _whatsAppSettings;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public GetVenueWhatsAppSettingsQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        IOptions<WhatsAppSettings> whatsAppSettings)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _whatsAppSettings = whatsAppSettings.Value;
    }

    /// <inheritdoc />
    public async Task<VenueWhatsAppSettingsDto> Handle(
        GetVenueWhatsAppSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var venue = await _dbContext.Venues
            .AsNoTracking()
            .Where(v => v.Id == request.VenueId && v.TenantId == tenantId && !v.IsDeleted)
            .Select(v => new
            {
                v.WhatsAppEnabled,
                v.WaNotifyReservationReceived,
                v.WaNotifyReservationConfirmed,
                v.WaNotifyReminder,
                v.WaNotifyCancellation
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
            throw new NotFoundException("Venue", request.VenueId);

        var isConnected = !string.IsNullOrWhiteSpace(_whatsAppSettings.AccountSid) &&
                          !string.IsNullOrWhiteSpace(_whatsAppSettings.AuthToken);

        return new VenueWhatsAppSettingsDto
        {
            WhatsAppEnabled = venue.WhatsAppEnabled,
            NotifyReservationReceived = venue.WaNotifyReservationReceived,
            NotifyReservationConfirmed = venue.WaNotifyReservationConfirmed,
            NotifyReminder = venue.WaNotifyReminder,
            NotifyCancellation = venue.WaNotifyCancellation,
            IsConnected = isConnected
        };
    }
}
