using MediatR;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Tenant kullanım istatistikleri sorgusu.
/// </summary>
public sealed record GetTenantUsageQuery : IRequest<TenantUsageDto>
{
}
