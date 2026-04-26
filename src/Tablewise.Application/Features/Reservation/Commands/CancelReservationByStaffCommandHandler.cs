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
/// CancelReservationByStaffCommand handler.
/// </summary>
public sealed class CancelReservationByStaffCommandHandler : IRequestHandler<CancelReservationByStaffCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ISlotAvailabilityService _slotService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CancelReservationByStaffCommandHandler> _logger;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public CancelReservationByStaffCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ISlotAvailabilityService slotService,
        IEmailService emailService,
        ILogger<CancelReservationByStaffCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _slotService = slotService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> Handle(CancelReservationByStaffCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations
            .Query()
            .Include(r => r.Venue)
            .Where(r => r.Id == request.ReservationId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (reservation == null)
        {
            throw new NotFoundException("Reservation", request.ReservationId.ToString(), "Rezervasyon bulunamadı.");
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new BusinessRuleException("Bu rezervasyon zaten iptal edilmiş.");
        }

        if (reservation.Status == ReservationStatus.Completed)
        {
            throw new BusinessRuleException("Tamamlanmış rezervasyon iptal edilemez.");
        }

        var oldStatus = reservation.Status;
        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancellationReason = request.Reason;
        reservation.CancelledAt = DateTime.UtcNow;

        // Kapora iadesi
        if (request.RefundDeposit && reservation.DepositStatus == DepositStatus.Paid)
        {
            // TODO: Faz 7'de İyzico refund
            reservation.DepositStatus = DepositStatus.Refunded;
            reservation.DepositRefundedAt = DateTime.UtcNow;
        }

        // Status log
        var statusLog = new ReservationStatusLog
        {
            ReservationId = reservation.Id,
            FromStatus = oldStatus,
            ToStatus = ReservationStatus.Cancelled,
            ChangedByUserId = _currentUser.UserId,
            ChangedBy = _currentUser.Email,
            Reason = $"İşletme tarafından iptal: {request.Reason}"
        };
        _unitOfWork.ReservationStatusLogs.Add(statusLog);

        // Audit log
        var auditLog = new AuditLog
        {
            TenantId = reservation.TenantId,
            EntityType = "Reservation",
            EntityId = reservation.Id,
            Action = "CancelledByStaff",
            PerformedBy = _currentUser.Email ?? "Staff",
            Details = request.Reason
        };
        _unitOfWork.AuditLogs.Add(auditLog);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Cache invalidate
        await _slotService.InvalidateCacheAsync(reservation.VenueId, reservation.ReservedFor.Date, cancellationToken)
            .ConfigureAwait(false);

        // Bildirim gönder
        if (request.SendNotification && !string.IsNullOrEmpty(reservation.GuestEmail))
        {
            _ = SendCancellationNotificationAsync(reservation);
        }

        _logger.LogInformation(
            "Rezervasyon işletme tarafından iptal edildi. Id: {Id}, By: {User}, Reason: {Reason}",
            reservation.Id, _currentUser.Email, request.Reason);

        return true;
    }

    private async Task SendCancellationNotificationAsync(Domain.Entities.Reservation reservation)
    {
        try
        {
            await _emailService.SendReservationCancellationAsync(
                reservation.GuestEmail!,
                reservation.GuestName,
                reservation.Venue?.Name ?? string.Empty,
                reservation.ReservedFor,
                reservation.ConfirmCode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İptal bildirimi gönderilemedi. Id: {Id}", reservation.Id);
        }
    }
}
