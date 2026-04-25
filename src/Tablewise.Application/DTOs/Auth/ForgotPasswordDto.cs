namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Şifre sıfırlama isteği DTO'su.
/// </summary>
public sealed record ForgotPasswordDto
{
    /// <summary>
    /// Şifre sıfırlama linki gönderilecek email adresi.
    /// </summary>
    public required string Email { get; init; }
}
