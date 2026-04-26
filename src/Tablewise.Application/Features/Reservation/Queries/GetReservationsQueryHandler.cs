using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Reservation;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Reservation.Queries;

/// <summary>
/// GetReservationsQuery handler.
/// </summary>
public sealed class GetReservationsQueryHandler : IRequestHandler<GetReservationsQuery, ReservationListResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public GetReservationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<ReservationListResponseDto> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Reservations
            .Query()
            .Include(r => r.Venue)
            .Include(r => r.Table)
            .Include(r => r.TableCombination)
            .Include(r => r.Customer)
            .AsQueryable();

        // Venue filter
        if (request.VenueId.HasValue)
        {
            query = query.Where(r => r.VenueId == request.VenueId.Value);
        }

        // Date range filter
        if (request.FromDate.HasValue)
        {
            query = query.Where(r => r.ReservedFor >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(r => r.ReservedFor <= request.ToDate.Value);
        }

        // Status filter
        if (!string.IsNullOrEmpty(request.Status))
        {
            var statuses = request.Status
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<ReservationStatus>(s.Trim(), true, out var status) ? status : (ReservationStatus?)null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();

            if (statuses.Count > 0)
            {
                query = query.Where(r => statuses.Contains(r.Status));
            }
        }

        // Table filter
        if (request.TableId.HasValue)
        {
            query = query.Where(r => r.TableId == request.TableId.Value);
        }

        // Search filter
        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(r =>
                r.GuestName.ToLower().Contains(searchTerm) ||
                r.GuestPhone.Contains(searchTerm) ||
                (r.GuestEmail != null && r.GuestEmail.ToLower().Contains(searchTerm)) ||
                r.ConfirmCode.ToLower().Contains(searchTerm));
        }

        // Total count
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Sorting
        query = request.SortBy.ToLower() switch
        {
            "reservedfor" => request.SortDirection.ToLower() == "desc"
                ? query.OrderByDescending(r => r.ReservedFor)
                : query.OrderBy(r => r.ReservedFor),
            "guestname" => request.SortDirection.ToLower() == "desc"
                ? query.OrderByDescending(r => r.GuestName)
                : query.OrderBy(r => r.GuestName),
            "createdat" => request.SortDirection.ToLower() == "desc"
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt),
            "status" => request.SortDirection.ToLower() == "desc"
                ? query.OrderByDescending(r => r.Status)
                : query.OrderBy(r => r.Status),
            "partysize" => request.SortDirection.ToLower() == "desc"
                ? query.OrderByDescending(r => r.PartySize)
                : query.OrderBy(r => r.PartySize),
            _ => request.SortDirection.ToLower() == "desc"
                ? query.OrderByDescending(r => r.ReservedFor)
                : query.OrderBy(r => r.ReservedFor)
        };

        // Pagination
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ReservationDto
            {
                Id = r.Id,
                VenueId = r.VenueId,
                VenueName = r.Venue != null ? r.Venue.Name : string.Empty,
                TableId = r.TableId,
                TableName = r.Table != null ? r.Table.Name : null,
                TableCombinationId = r.TableCombinationId,
                TableCombinationName = r.TableCombination != null ? r.TableCombination.Name : null,
                CustomerId = r.CustomerId,
                GuestName = r.GuestName,
                GuestEmail = r.GuestEmail,
                GuestPhone = r.GuestPhone,
                CustomerTier = r.Customer != null ? r.Customer.Tier.ToString() : null,
                PartySize = r.PartySize,
                ReservedFor = r.ReservedFor,
                EndTime = r.EndTime,
                Status = r.Status.ToString(),
                Source = r.Source.ToString(),
                ConfirmCode = r.ConfirmCode,
                SpecialRequests = r.SpecialRequests,
                InternalNotes = r.InternalNotes,
                DiscountPercent = r.DiscountPercent,
                DepositStatus = r.DepositStatus.ToString(),
                DepositAmount = r.DepositAmount,
                DepositPaidAt = r.DepositPaidAt,
                CancellationReason = r.CancellationReason,
                CancelledAt = r.CancelledAt,
                CreatedAt = r.CreatedAt,
                CustomFieldAnswers = string.IsNullOrEmpty(r.CustomFieldAnswers)
                    ? null
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(r.CustomFieldAnswers),
                ModifiedFromReservationId = r.ModifiedFromReservationId
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ReservationListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
