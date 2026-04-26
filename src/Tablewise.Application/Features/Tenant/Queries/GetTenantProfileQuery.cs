using MediatR;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Tenant profil sorgusu.
/// </summary>
public sealed record GetTenantProfileQuery : IRequest<TenantProfileDto>
{
}
