using MediatR;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed record UpdateTenantPlanCommand(Guid TenantId, Guid PlanId) : IRequest<Unit>;
