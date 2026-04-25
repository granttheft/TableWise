namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Yeni şifre belirleme isteği DTO'su.
/// </summary>
public sealed record ResetPasswordDto
{
    /// <summary>
    /// Şifre sıfırlama token'ı (URL'den gelen).
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Yeni şifre.
    /// </summary>
    public required string NewPassword { get; init; }

    /// <summary>
    /// Yeni şifre tekrarı.
    /// </summary>
    public required string ConfirmPassword { get; init; }
}
