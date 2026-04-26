namespace Tablewise.Application.DTOs.Staff;

/// <summary>
/// Davet kabul isteği DTO'su.
/// </summary>
public sealed record AcceptInvitationDto
{
    /// <summary>
    /// Ad.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Soyad.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Şifre.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Şifre tekrarı.
    /// </summary>
    public required string ConfirmPassword { get; init; }

    /// <summary>
    /// Telefon numarası (opsiyonel).
    /// </summary>
    public string? PhoneNumber { get; init; }
}
