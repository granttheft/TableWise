namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Email doğrulama isteği DTO'su.
/// </summary>
public sealed record VerifyEmailDto
{
    /// <summary>
    /// Email doğrulama token'ı (URL'den gelen).
    /// </summary>
    public required string Token { get; init; }
}
