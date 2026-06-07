using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Common;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Features.Platform.Commands;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, PagedResult<CouponDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCouponsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<CouponDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Coupons.Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Code.Contains(request.Search.ToUpperInvariant()));

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => CreateCouponCommandHandler.Map(c))
            .ToListAsync(cancellationToken);

        return new PagedResult<CouponDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }
}
