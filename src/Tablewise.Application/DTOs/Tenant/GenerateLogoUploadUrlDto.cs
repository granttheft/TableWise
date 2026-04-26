namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Logo upload URL oluşturma request DTO'su.
/// </summary>
public sealed record GenerateLogoUploadUrlDto
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
