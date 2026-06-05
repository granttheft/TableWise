using MediatR;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed record AddPlatformNoteCommand(
    Guid TenantId,
    string Content,
    string CreatedByEmail
) : IRequest<Unit>;
