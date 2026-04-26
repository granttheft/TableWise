using MediatR;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Logo upload onaylama komutu.
/// Upload tamamlandıktan sonra çağrılır.
/// </summary>
public sealed record ConfirmLogoUploadCommand : IRequest<Unit>
{
    /// <summary>
    /// Upload edilen dosyanın R2 key'i.
    /// </summary>
    public required string FileKey { get; init; }
}
