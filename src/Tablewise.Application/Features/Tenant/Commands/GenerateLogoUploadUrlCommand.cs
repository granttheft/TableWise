using MediatR;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Logo upload için presigned URL oluşturma komutu.
/// Sadece Owner rolü kullanabilir.
/// </summary>
public sealed record GenerateLogoUploadUrlCommand : IRequest<LogoUploadUrlDto>
{
    /// <summary>
    /// Dosya MIME tipi (image/jpeg, image/png, image/webp).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Dosya boyutu (bytes).
    /// </summary>
    public required long FileSizeBytes { get; init; }
}
