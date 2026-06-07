using MediatR;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.DTOs.Common;

namespace Tablewise.Application.Features.Platform.Queries;

public record GetCouponsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null) : IRequest<PagedResult<CouponDto>>;
