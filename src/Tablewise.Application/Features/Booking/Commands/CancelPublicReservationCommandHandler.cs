using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Booking.Commands;

/// <summary>
/// CancelPublicReservationCommand handler.
/// </summary>
public sealed class CancelPublicReservationCommandHandler : IRequestHandler<CancelPublicReservationCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlotAvailabilityService _slotService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CancelPublicReservationCommandHandler> _logger;

    private const int CancellationDeadlineHours = 24;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public CancelPublicReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ISlotAvailabilityService slotService,
        IEmailService emailService,
        ILogger<CancelPublicReservationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _slotService = slotService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> Handle(CancelPublicReservationCommand request, CancellationToken cancellationToken)
    {
        // 1. Rezervasyonu bul
        var reservation = await _unitOfWork.Reservations
            .Query()
            .IgnoreQueryFilters()
            .Include(r => r.Venue)
            .Where(r => r.ConfirmCode == request.ConfirmCode && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (reservation == null)
        {
            throw new NotFoundException("Reservation", request.ConfirmCode, "Rezervasyon bulunamadı.");
        }

        // 2. Durum kontrolü
        if (reservation.Status != ReservationStatus.Confirmed && reservation.Status != ReservationStatus.Pending)
        {
            throw new BusinessRuleException("Bu rezervasyon iptal edilemez. Mevcut durum: " + reservation.Status);
        }

        // 3. 24 saat kontrolü
        var hoursUntilReservation = (reservation.ReservedFor - DateTime.UtcNow).TotalHours;
        if (hoursUntilReservation < CancellationDeadlineHours)
        {
            throw new BusinessRuleException(
                $"Rezervasyon iptali en az {CancellationDeadlineHours} saat öncesinden yapılmalıdır. " +
                $"Kalan süre: {hoursUntilReservation:F1} saat. " +
                "İptal için lütfen mekan ile iletişime geçin.");
        }

        // 4. İptal işlemi
        var oldStatus = reservation.Status;
        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancellationReason = request.Reason ?? "Müşteri tarafından iptal edildi";
        reservation.CancelledAt = DateTime.UtcNow;

        // 5. Status log
        var statusLog = new ReservationStatusLog
        {
            ReservationId = reservation.Id,
            FromStatus = oldStatus,
            ToStatus = ReservationStatus.Cancelled,
            ChangedBy = "Customer",
            Reason = reservation.CancellationReason
        };
        await _unitOfWork.ReservationStatusLogs.AddAsync(statusLog, cancellationToken).ConfigureAwait(false);

        // 6. Audit log
        var auditLog = new AuditLog
        {
            TenantId = reservation.TenantId,
            EntityType = "Reservation",
            EntityId = reservation.Id.ToString(),
            Action = "CancelledByCustomer",
            PerformedBy = "Customer",
            NewValue = $"CancellationReason: {reservation.CancellationReason}",
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken).ConfigureAwait(false);

        // 7. Kapora iadesi (Faz 7)
        if (reservation.DepositStatus == DepositStatus.Paid)
        {
            _logger.LogInformation("Kapora iadesi başlatıldı. ReservationId: {ReservationId}", reservation.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // 8. Cache invalidate
        await _slotService.InvalidateCacheAsync(reservation.VenueId, reservation.ReservedFor.Date, cancellationToken)
            .ConfigureAwait(false);

        // 9. Email gönder
        _ = SendCancellationEmailAsync(reservation);

        _logger.LogInformation(
            "Rezervasyon müşteri tarafından iptal edildi. Id: {ReservationId}, ConfirmCode: {ConfirmCode}",
            reservation.Id, reservation.ConfirmCode);

        return true;
    }

    private async Task SendCancellationEmailAsync(Domain.Entities.Reservation reservation)
    {
        if (string.IsNullOrEmpty(reservation.GuestEmail))
            return;

        try
        {
            await _emailService.SendReservationCancellationAsync(
                reservation.GuestEmail,
                reservation.GuestName,
                reservation.Venue?.Name ?? string.Empty,
                reservation.ReservedFor,
                reservation.ConfirmCode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İptal email'i gönderilemedi. ReservationId: {ReservationId}", reservation.Id);
        }
    }
}
