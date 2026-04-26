namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Logo upload onaylama DTO'su.
/// </summary>
public sealed record ConfirmLogoUploadDto
{
    /// <summary>
    /// Upload edilen dosyanın R2 key'i.
    /// </summary>
    public required string FileKey { get; init; }
}
