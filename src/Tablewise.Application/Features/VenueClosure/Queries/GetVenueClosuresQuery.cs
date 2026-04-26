using MediatR;
using Tablewise.Application.DTOs.VenueClosure;

namespace Tablewise.Application.Features.VenueClosure.Queries;

/// <summary>
/// Venue kapalılık listesi sorgusu.
/// Yıllık kapalılık günlerini getirir.
/// </summary>
public sealed record GetVenueClosuresQuery : IRequest<List<VenueClosureDto>>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Başlangıç tarihi (opsiyonel). Varsayılan: bugünden 1 yıl sonra.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Bitiş tarihi (opsiyonel). Varsayılan: bugünden 1 yıl sonra.
    /// </summary>
    public DateTime? EndDate { get; init; }
}
