namespace Tablewise.Application.DTOs.Auth;

/// <summary>
/// Tenant kayıt isteği DTO'su.
/// </summary>
public sealed record RegisterTenantDto
{
    /// <summary>
    /// İşletme/organizasyon adı.
    /// </summary>
    public required string BusinessName { get; init; }

    /// <summary>
    /// Owner kullanıcı email adresi.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Şifre (min 8 karakter, büyük/küçük harf, rakam).
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Şifre tekrarı.
    /// </summary>
    public required string ConfirmPassword { get; init; }

    /// <summary>
    /// Owner kullanıcı adı.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Owner kullanıcı soyadı.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Telefon numarası (opsiyonel).
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// KVKK/GDPR onay checkbox (zorunlu true).
    /// </summary>
    public bool AcceptTerms { get; init; }
}
