using MediatR;
using Tablewise.Application.DTOs.Venue;

namespace Tablewise.Application.Features.Venue.Queries;

/// <summary>
/// Venue WhatsApp bildirim ayarlarını getirir.
/// </summary>
public sealed record GetVenueWhatsAppSettingsQuery(Guid VenueId) : IRequest<VenueWhatsAppSettingsDto>;
