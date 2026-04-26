using MediatR;

namespace Tablewise.Application.Features.Venue.Commands;

/// <summary>
/// Venue çalışma saatleri güncelleme komutu.
/// Sadece Owner rolü kullanabilir.
/// </summary>
public sealed record UpdateWorkingHoursCommand : IRequest<Unit>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Çalışma saatleri (JSON formatında).
    /// </summary>
    public required string WorkingHours { get; init; }
}
