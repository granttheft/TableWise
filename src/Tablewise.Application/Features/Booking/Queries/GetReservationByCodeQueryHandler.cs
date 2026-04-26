using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Booking;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Booking.Queries;

/// <summary>
/// GetReservationByCodeQuery handler.
/// </summary>
public sealed class GetReservationByCodeQueryHandler : IRequestHandler<GetReservationByCodeQuery, ReservationDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    private const int ModificationDeadlineHours = 24;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public GetReservationByCodeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ReservationDetailDto> Handle(GetReservationByCodeQuery request, CancellationToken cancellationToken)
    {
        // Tenant filter'ı bypass et (public endpoint)
        var reservation = await _unitOfWork.Reservations
            .Query()
            .IgnoreQueryFilters()
            .Include(r => r.Venue)
            .Include(r => r.Table)
            .Include(r => r.TableCombination)
            .Where(r => r.ConfirmCode == request.ConfirmCode && !r.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (reservation == null)
        {
            throw new NotFoundException("Reservation", request.ConfirmCode, "Rezervasyon bulunamadı.");
        }

        // Değiştirme/iptal için deadline hesapla
        var now = DateTime.UtcNow;
        var hoursUntilReservation = (reservation.ReservedFor - now).TotalHours;
        var canModify = reservation.Status == ReservationStatus.Confirmed &&
                       hoursUntilReservation >= ModificationDeadlineHours;
        var canCancel = (reservation.Status == ReservationStatus.Confirmed || reservation.Status == ReservationStatus.Pending) &&
                       hoursUntilReservation >= ModificationDeadlineHours;

        var hoursUntilDeadline = hoursUntilReservation >= ModificationDeadlineHours
            ? (int)(hoursUntilReservation - ModificationDeadlineHours)
            : 0;

        return new ReservationDetailDto
        {
            ConfirmCode = reservation.ConfirmCode,
            Status = reservation.Status.ToString(),
            GuestName = reservation.GuestName,
            ReservedFor = reservation.ReservedFor,
            EndTime = reservation.EndTime,
            PartySize = reservation.PartySize,
            VenueName = reservation.Venue?.Name ?? string.Empty,
            VenueAddress = reservation.Venue?.Address,
            VenuePhone = reservation.Venue?.PhoneNumber,
            TableName = reservation.Table?.Name ?? reservation.TableCombination?.Name,
            SpecialRequests = reservation.SpecialRequests,
            DepositStatus = reservation.DepositStatus.ToString(),
            DepositAmount = reservation.DepositAmount,
            CanModify = canModify,
            CanCancel = canCancel,
            HoursUntilDeadline = hoursUntilDeadline > 0 ? hoursUntilDeadline : null
        };
    }
}
