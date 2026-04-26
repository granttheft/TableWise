using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Reservation;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Reservation.Queries;

/// <summary>
/// GetReservationByIdQuery handler.
/// </summary>
public sealed class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationDto>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public GetReservationByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ReservationDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations
            .Query()
            .Include(r => r.Venue)
            .Include(r => r.Table)
            .Include(r => r.TableCombination)
            .Include(r => r.Customer)
            .Where(r => r.Id == request.ReservationId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (reservation == null)
        {
            throw new NotFoundException("Reservation", request.ReservationId.ToString(), "Rezervasyon bulunamadı.");
        }

        return new ReservationDto
        {
            Id = reservation.Id,
            VenueId = reservation.VenueId,
            VenueName = reservation.Venue?.Name ?? string.Empty,
            TableId = reservation.TableId,
            TableName = reservation.Table?.Name,
            TableCombinationId = reservation.TableCombinationId,
            TableCombinationName = reservation.TableCombination?.Name,
            CustomerId = reservation.CustomerId,
            GuestName = reservation.GuestName,
            GuestEmail = reservation.GuestEmail,
            GuestPhone = reservation.GuestPhone,
            CustomerTier = reservation.Customer?.Tier.ToString(),
            PartySize = reservation.PartySize,
            ReservedFor = reservation.ReservedFor,
            EndTime = reservation.EndTime,
            Status = reservation.Status.ToString(),
            Source = reservation.Source.ToString(),
            ConfirmCode = reservation.ConfirmCode,
            SpecialRequests = reservation.SpecialRequests,
            InternalNotes = reservation.InternalNotes,
            DiscountPercent = reservation.DiscountPercent,
            DepositStatus = reservation.DepositStatus.ToString(),
            DepositAmount = reservation.DepositAmount,
            DepositPaidAt = reservation.DepositPaidAt,
            CancellationReason = reservation.CancellationReason,
            CancelledAt = reservation.CancelledAt,
            CreatedAt = reservation.CreatedAt,
            CustomFieldAnswers = string.IsNullOrEmpty(reservation.CustomFieldAnswers)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, string>>(reservation.CustomFieldAnswers),
            ModifiedFromReservationId = reservation.ModifiedFromReservationId
        };
    }
}
