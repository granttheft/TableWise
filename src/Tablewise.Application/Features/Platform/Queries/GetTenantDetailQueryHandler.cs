using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed class GetTenantDetailQueryHandler : IRequestHandler<GetTenantDetailQuery, TenantDetailDto>
{
    private readonly IApplicationDbContext _db;

    public GetTenantDetailQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<TenantDetailDto> Handle(GetTenantDetailQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Id == request.TenantId && !t.IsDeleted)
            .Include(t => t.Plan)
            .Include(t => t.Users.Where(u => !u.IsDeleted))
            .Include(t => t.Venues.Where(v => !v.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var notes = await _db.PlatformNotes
            .IgnoreQueryFilters()
            .Where(n => n.TenantId == request.TenantId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new PlatformNoteDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedByEmail = n.CreatedByEmail,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new TenantDetailDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Email = tenant.Email,
            Slug = tenant.Slug,
            PlanName = tenant.Plan?.Name ?? "Bilinmiyor",
            PlanStatus = tenant.PlanStatus,
            CreatedAt = tenant.CreatedAt,
            ReservationCountThisMonth = tenant.ReservationCountThisMonth,
            IsActive = tenant.IsActive,
            TrialEndsAt = tenant.TrialEndsAt,
            PlanRenewsAt = tenant.PlanRenewsAt,
            VenueCount = tenant.Venues.Count,
            UserCount = tenant.Users.Count,
            Notes = notes,
            CustomLimitsJson = tenant.CustomLimitsJson
        };
    }
}
