using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Tenant venue listesi query'si
/// </summary>
public sealed class GetTenantVenuesQuery : IRequest<List<TenantVenueDto>>
{
}

/// <summary>
/// GetTenantVenuesQuery handler
/// </summary>
public sealed class GetTenantVenuesQueryHandler : IRequestHandler<GetTenantVenuesQuery, List<TenantVenueDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetTenantVenuesQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<List<TenantVenueDto>> Handle(GetTenantVenuesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var venues = await _context.Venues
            .Where(v => v.TenantId == tenantId && !v.IsDeleted)
            .Select(v => new TenantVenueDto
            {
                Id = v.Id,
                Name = v.Name,
                Slug = v.Name.ToLower().Replace(" ", "-"), // Slug generate (geçici)
                IsActive = true, // Şimdilik tüm venue'ler aktif
                TableCount = v.Tables.Count(t => !t.IsDeleted)
            })
            .OrderBy(v => v.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return venues;
    }
}
