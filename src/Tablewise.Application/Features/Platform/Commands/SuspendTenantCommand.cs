using MediatR;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed record SuspendTenantCommand(Guid TenantId, bool Suspend) : IRequest<Unit>;
