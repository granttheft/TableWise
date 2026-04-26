using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Booking;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Booking.Queries;

/// <summary>
/// GetAvailableSlotsQuery handler.
/// </summary>
public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, AvailabilityResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlotAvailabilityService _slotService;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public GetAvailableSlotsQueryHandler(
        IUnitOfWork unitOfWork,
        ISlotAvailabilityService slotService)
    {
        _unitOfWork = unitOfWork;
        _slotService = slotService;
    }

    /// <inheritdoc />
    public async Task<AvailabilityResponseDto> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        // Slug ile venue bul
        var venue = await _unitOfWork.Venues
            .Query()
            .IgnoreQueryFilters()
            .Include(v => v.Tenant)
            .Where(v => v.Tenant != null &&
                        v.Tenant.Slug == request.Slug &&
                        !v.Tenant.IsDeleted &&
                        v.Tenant.IsActive &&
                        !v.IsDeleted)
            .Select(v => new { v.Id, v.TimeZone })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.Slug, "Mekan bulunamadı.");
        }

        // Kapalılık kontrolü
        var closure = await _unitOfWork.VenueClosures
            .Query()
            .IgnoreQueryFilters()
            .Where(c => c.VenueId == venue.Id && c.Date.Date == request.Date.Date && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (closure?.IsFullDay == true)
        {
            return new AvailabilityResponseDto
            {
                VenueId = venue.Id,
                Date = request.Date.Date,
                PartySize = request.PartySize,
                Slots = [],
                IsVenueClosed = true,
                ClosureReason = closure.Reason ?? "Mekan bu tarihte kapalı."
            };
        }

        // Müsait slotları al
        var availableSlots = await _slotService.GetAvailableSlotsAsync(
            venue.Id,
            request.Date,
            request.PartySize,
            request.TableId,
            cancellationToken)
            .ConfigureAwait(false);

        // DTO'ya dönüştür
        var slots = availableSlots.Select(s => new SlotDto
        {
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            TimeLabel = s.StartTime.ToString("HH:mm"),
            AvailableTableCount = s.AvailableTables.Count + s.AvailableCombinations.Count,
            AvailableTables = s.AvailableTables.Select(t => new TableOptionDto
            {
                TableId = t.TableId,
                Name = t.Name,
                Capacity = t.Capacity,
                Location = t.Location
            }).ToList(),
            AvailableCombinations = s.AvailableCombinations.Select(c => new TableCombinationOptionDto
            {
                CombinationId = c.CombinationId,
                Name = c.Name,
                CombinedCapacity = c.CombinedCapacity
            }).ToList()
        }).ToList();

        return new AvailabilityResponseDto
        {
            VenueId = venue.Id,
            Date = request.Date.Date,
            PartySize = request.PartySize,
            Slots = slots,
            IsVenueClosed = false
        };
    }
}
