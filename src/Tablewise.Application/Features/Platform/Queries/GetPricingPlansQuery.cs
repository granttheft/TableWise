using MediatR;
using Tablewise.Application.DTOs.Platform;

namespace Tablewise.Application.Features.Platform.Queries;

public record GetPricingPlansQuery : IRequest<IReadOnlyList<PlanPricingDto>>;
