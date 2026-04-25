namespace Tablewise.Infrastructure.Auth;

/// <summary>
/// JWT yapılandırma ayarları.
/// appsettings.json'dan okunur.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Configuration section adı.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key (HS256 için). Min 32 karakter.
    /// Faz 9'da RS256'ya geçilecek.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer (iss claim).
    /// </summary>
    public string Issuer { get; set; } = "tablewise.com.tr";

    /// <summary>
    /// Token audience (aud claim).
    /// </summary>
    public string Audience { get; set; } = "tablewise-api";

    /// <summary>
    /// Access token geçerlilik süresi (dakika).
    /// Default: 60 dakika.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token geçerlilik süresi (gün).
    /// Default: 30 gün.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 30;

    /// <summary>
    /// "Remember me" için uzatılmış refresh token süresi (gün).
    /// Default: 90 gün.
    /// </summary>
    public int ExtendedRefreshTokenExpirationDays { get; set; } = 90;

    /// <summary>
    /// Clock skew toleransı (saniye). Token expiry kontrolünde kullanılır.
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 30;
}
