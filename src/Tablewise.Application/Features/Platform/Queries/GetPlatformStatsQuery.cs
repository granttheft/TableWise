using MediatR;
using Tablewise.Application.DTOs.Platform;

namespace Tablewise.Application.Features.Platform.Queries;

public sealed record GetPlatformStatsQuery : IRequest<PlatformStatsDto>;
