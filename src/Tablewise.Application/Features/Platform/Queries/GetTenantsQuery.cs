using MediatR;
using Tablewise.Application.DTOs.Common;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed record GetTenantsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    PlanStatus? Status = null
) : IRequest<PagedResult<TenantSummaryDto>>;
