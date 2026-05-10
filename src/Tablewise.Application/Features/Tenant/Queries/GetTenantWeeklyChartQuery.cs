using MediatR;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Dashboard haftalık grafik verisi sorgusu (son 7 gün, UTC).
/// </summary>
public sealed record GetTenantWeeklyChartQuery : IRequest<List<WeeklyChartPointDto>>;
