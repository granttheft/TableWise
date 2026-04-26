using MediatR;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// Rezervasyona internal not ekleme komutu.
/// </summary>
public sealed record AddInternalNoteCommand : IRequest<bool>
{
    /// <summary>
    /// Rezervasyon ID.
    /// </summary>
    public Guid ReservationId { get; init; }

    /// <summary>
    /// Not içeriği.
    /// </summary>
    public string Note { get; init; } = string.Empty;
}
