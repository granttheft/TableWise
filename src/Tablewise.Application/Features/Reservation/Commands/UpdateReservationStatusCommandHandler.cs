using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Reservation.Commands;

/// <summary>
/// UpdateReservationStatusCommand handler.
/// </summary>
public sealed class UpdateReservationStatusCommandHandler : IRequestHandler<UpdateReservationStatusCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ISlotAvailabilityService _slotService;
    private readonly ILogger<UpdateReservationStatusCommandHandler> _logger;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public UpdateReservationStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ISlotAvailabilityService slotService,
        ILogger<UpdateReservationStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _slotService = slotService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> Handle(UpdateReservationStatusCommand request, CancellationToken cancellationToken)
    {
        // Parse status
        if (!Enum.TryParse<ReservationStatus>(request.Status, true, out var newStatus))
        {
            throw new BusinessRuleException($"Geçersiz durum: {request.Status}");
        }

        // Reservation bul
        var reservation = await _unitOfWork.Reservations
            .Query()
            .Where(r => r.Id == request.ReservationId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (reservation == null)
        {
            throw new NotFoundException("Reservation", request.ReservationId.ToString(), "Rezervasyon bulunamadı.");
        }

        // Geçerli geçiş kontrolü
        if (!IsValidTransition(reservation.Status, newStatus))
        {
            throw new BusinessRuleException(
                $"Geçersiz durum geçişi: {reservation.Status} -> {newStatus}");
        }

        var oldStatus = reservation.Status;
        reservation.Status = newStatus;

        // Tamamlandı veya NoShow ise customer stats güncelle
        if (newStatus == ReservationStatus.Completed && reservation.CustomerId.HasValue)
        {
            var customer = await _unitOfWork.Customers
                .Query()
                .Where(c => c.Id == reservation.CustomerId.Value)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (customer != null)
            {
                customer.TotalVisits++;
                customer.LastReservationAt = reservation.ReservedFor;
            }
        }

        // Status log
        var statusLog = new ReservationStatusLog
        {
            ReservationId = reservation.Id,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangedByUserId = _currentUser.UserId,
            ChangedBy = _currentUser.Email,
            Reason = request.Reason
        };
        _unitOfWork.ReservationStatusLogs.Add(statusLog);

        // Audit log
        var auditLog = new AuditLog
        {
            TenantId = reservation.TenantId,
            EntityType = "Reservation",
            EntityId = reservation.Id,
            Action = $"StatusChanged:{oldStatus}->{newStatus}",
            PerformedBy = _currentUser.Email ?? "Staff",
            Details = request.Reason
        };
        _unitOfWork.AuditLogs.Add(auditLog);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Cache invalidate (iptal durumunda)
        if (newStatus == ReservationStatus.Cancelled)
        {
            await _slotService.InvalidateCacheAsync(reservation.VenueId, reservation.ReservedFor.Date, cancellationToken)
                .ConfigureAwait(false);
        }

        _logger.LogInformation(
            "Rezervasyon durumu güncellendi. Id: {Id}, {Old} -> {New}, By: {User}",
            reservation.Id, oldStatus, newStatus, _currentUser.Email);

        return true;
    }

    private static bool IsValidTransition(ReservationStatus from, ReservationStatus to)
    {
        return (from, to) switch
        {
            (ReservationStatus.Pending, ReservationStatus.Confirmed) => true,
            (ReservationStatus.Pending, ReservationStatus.Cancelled) => true,
            (ReservationStatus.Confirmed, ReservationStatus.Completed) => true,
            (ReservationStatus.Confirmed, ReservationStatus.NoShow) => true,
            (ReservationStatus.Confirmed, ReservationStatus.Cancelled) => true,
            (ReservationStatus.Confirmed, ReservationStatus.Modified) => true,
            _ => false
        };
    }
}
