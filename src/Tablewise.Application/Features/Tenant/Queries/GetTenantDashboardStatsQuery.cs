using MediatR;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Dashboard üst istatistik kartları sorgusu.
/// </summary>
public sealed record GetTenantDashboardStatsQuery : IRequest<TenantDashboardStatsDto>;
