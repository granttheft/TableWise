namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Token response DTO'su. Access ve refresh token içerir.
/// </summary>
public sealed record TokenResponseDto
{
    /// <summary>
    /// JWT access token.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh token (rotation için).
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Access token son kullanma tarihi (UTC).
    /// </summary>
    public required DateTime AccessTokenExpiresAt { get; init; }

    /// <summary>
    /// Refresh token son kullanma tarihi (UTC).
    /// </summary>
    public required DateTime RefreshTokenExpiresAt { get; init; }

    /// <summary>
    /// Token tipi (her zaman "Bearer").
    /// </summary>
    public string TokenType => "Bearer";
}
