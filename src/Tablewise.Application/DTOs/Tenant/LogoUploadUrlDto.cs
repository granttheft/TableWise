namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Logo upload response DTO'su.
/// </summary>
public sealed record LogoUploadUrlDto
{
    /// <summary>
    /// R2 presigned upload URL.
    /// </summary>
    public required string UploadUrl { get; init; }

    /// <summary>
    /// Upload key (confirm için gerekli).
    /// </summary>
    public required string FileKey { get; init; }

    /// <summary>
    /// URL son kullanma tarihi.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Maksimum dosya boyutu (bytes).
    /// </summary>
    public required long MaxFileSizeBytes { get; init; }

    /// <summary>
    /// İzin verilen dosya tipleri.
    /// </summary>
    public required string[] AllowedContentTypes { get; init; }
}
