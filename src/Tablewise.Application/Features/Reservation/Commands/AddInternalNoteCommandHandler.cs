using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// AddInternalNoteCommand handler.
/// </summary>
public sealed class AddInternalNoteCommandHandler : IRequestHandler<AddInternalNoteCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public AddInternalNoteCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<bool> Handle(AddInternalNoteCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations
            .Query()
            .Where(r => r.Id == request.ReservationId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (reservation == null)
        {
            throw new NotFoundException("Reservation", request.ReservationId.ToString(), "Rezervasyon bulunamadı.");
        }

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var author = _currentUser.Email ?? "Staff";
        var newNote = $"[{timestamp}] {author}: {request.Note}";

        if (string.IsNullOrEmpty(reservation.InternalNotes))
        {
            reservation.InternalNotes = newNote;
        }
        else
        {
            reservation.InternalNotes += "\n" + newNote;
        }

        var auditLog = new AuditLog
        {
            TenantId = reservation.TenantId,
            EntityType = "Reservation",
            EntityId = reservation.Id.ToString(),
            Action = "NoteAdded",
            PerformedBy = author,
            NewValue = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
