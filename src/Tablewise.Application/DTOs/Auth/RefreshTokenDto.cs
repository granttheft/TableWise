namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Refresh token isteği DTO'su.
/// </summary>
public sealed record RefreshTokenDto
{
    /// <summary>
    /// Mevcut refresh token.
    /// </summary>
    public required string RefreshToken { get; init; }
}
