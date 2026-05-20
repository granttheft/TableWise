using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Booking;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Booking.Queries;

/// <summary>
/// GetVenueConfigQuery handler.
/// </summary>
public sealed class GetVenueConfigQueryHandler : IRequestHandler<GetVenueConfigQuery, VenueConfigDto>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public GetVenueConfigQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<VenueConfigDto> Handle(GetVenueConfigQuery request, CancellationToken cancellationToken)
    {
        // Slug ile tenant + venue bul (ignore tenant filter)
        var venue = await _unitOfWork.Venues
            .Query()
            .IgnoreQueryFilters()
            .Include(v => v.Tenant)
            .Include(v => v.CustomFields.Where(cf => !cf.IsDeleted && cf.IsPublic))
            .Where(v => v.Tenant != null &&
                        v.Tenant.Slug == request.Slug &&
                        !v.Tenant.IsDeleted &&
                        v.Tenant.IsActive &&
                        !v.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (venue == null)
        {
            throw new NotFoundException("Venue", request.Slug, "Mekan bulunamadı.");
        }

        // Çalışma saatlerini parse et
        Dictionary<string, WorkingHoursPeriod>? workingHours = null;
        if (!string.IsNullOrEmpty(venue.WorkingHours))
        {
            try
            {
                workingHours = JsonSerializer.Deserialize<Dictionary<string, WorkingHoursPeriod>>(
                    venue.WorkingHours,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // Parse hatası, varsayılan değerler kullanılacak
            }
        }

        // Onumuzdeki 60 gun tam gun kapaliliklar
        var today = DateTime.UtcNow.Date;
        var horizon = today.AddDays(60);
        var closures = await _unitOfWork.VenueClosures
            .Query()
            .IgnoreQueryFilters()
            .Where(c =>
                c.VenueId == venue.Id &&
                !c.IsDeleted &&
                c.IsFullDay &&
                c.Date.Date >= today &&
                c.Date.Date <= horizon)
            .OrderBy(c => c.Date)
            .Select(c => new BookingClosureDto
            {
                Id = c.Id,
                StartDate = c.Date.ToString("yyyy-MM-dd"),
                EndDate = c.Date.ToString("yyyy-MM-dd"),
                Reason = c.Reason ?? "Mekan kapali"
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // Custom field'ları map et
        var customFields = venue.CustomFields
            .OrderBy(cf => cf.SortOrder)
            .Select(cf => new VenueCustomFieldDto
            {
                FieldId = cf.Id,
                FieldKey = cf.Name,
                Label = cf.Label,
                Name = cf.Label,
                FieldType = cf.FieldType.ToString(),
                IsRequired = cf.IsRequired,
                Options = ParseOptions(cf.Options),
                Placeholder = cf.Placeholder
            })
            .ToList();

        return new VenueConfigDto
        {
            VenueId = venue.Id,
            Name = venue.Name,
            Slug = venue.Tenant!.Slug,
            Description = venue.Description,
            LogoUrl = venue.LogoUrl,
            Address = venue.Address,
            PhoneNumber = venue.PhoneNumber,
            SlotDurationMinutes = venue.SlotDurationMinutes,
            DepositEnabled = venue.DepositEnabled,
            DepositAmount = venue.DepositAmount,
            DepositPerPerson = venue.DepositPerPerson,
            WorkingHours = workingHours,
            Closures = closures,
            CustomFields = customFields,
            MinPartySize = 1,
            MaxPartySize = 20,
            MinAdvanceBookingDays = 0,
            MaxAdvanceBookingDays = 30
        };
    }

    private static IReadOnlyList<string>? ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrEmpty(optionsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson);
        }
        catch
        {
            return null;
        }
    }
}
