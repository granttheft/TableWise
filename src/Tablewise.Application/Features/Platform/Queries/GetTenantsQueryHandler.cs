using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Common;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, PagedResult<TenantSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetTenantsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<TenantSummaryDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(1, request.Page);

        var query = _db.Tenants
            .IgnoreQueryFilters()
            .Where(t => !t.IsDeleted)
            .Include(t => t.Plan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(search) || t.Email.ToLower().Contains(search));
        }

        if (request.Status.HasValue)
            query = query.Where(t => t.PlanStatus == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TenantSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email,
                PlanName = t.Plan != null ? t.Plan.Name : "Bilinmiyor",
                PlanStatus = t.PlanStatus,
                CreatedAt = t.CreatedAt,
                ReservationCountThisMonth = t.ReservationCountThisMonth,
                IsActive = t.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TenantSummaryDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
