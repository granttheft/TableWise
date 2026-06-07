using MediatR;
using Tablewise.Application.DTOs.Platform;

namespace Tablewise.Application.Features.Platform.Commands;

public record UpdatePlanPricingCommand(Guid PlanId, UpdatePlanPricingDto Dto) : IRequest<PlanPricingDto>;
