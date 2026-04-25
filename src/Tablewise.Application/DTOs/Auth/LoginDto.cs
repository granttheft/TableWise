namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Giriş isteği DTO'su.
/// </summary>
public sealed record LoginDto
{
    /// <summary>
    /// Kullanıcı email adresi.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Şifre.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Beni hatırla (refresh token süresini uzatır).
    /// </summary>
    public bool RememberMe { get; init; }
}
