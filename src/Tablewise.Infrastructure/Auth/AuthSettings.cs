namespace Tablewise.Infrastructure.Auth;

/// <summary>
/// Authentication yapılandırma ayarları.
/// </summary>
public sealed class AuthSettings
{
    /// <summary>
    /// Configuration section adı.
    /// </summary>
    public const string SectionName = "Auth";

    /// <summary>
    /// Brute-force koruma: Maksimum başarısız giriş denemesi.
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Brute-force koruma: Kilitleme süresi (dakika).
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 15;

    /// <summary>
    /// BCrypt work factor. Yüksek = daha güvenli ama yavaş.
    /// </summary>
    public int BcryptWorkFactor { get; set; } = 12;

    /// <summary>
    /// Trial süresi (gün).
    /// </summary>
    public int TrialDays { get; set; } = 14;

    /// <summary>
    /// Email doğrulama token geçerlilik süresi (saat).
    /// </summary>
    public int EmailVerificationTokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// Şifre sıfırlama token geçerlilik süresi (saat).
    /// </summary>
    public int PasswordResetTokenExpirationHours { get; set; } = 2;

    /// <summary>
    /// Admin panel URL (email linkleri için).
    /// </summary>
    public string AdminPanelUrl { get; set; } = "https://app.tablewise.com.tr";

    /// <summary>
    /// Destek email adresi.
    /// </summary>
    public string SupportEmail { get; set; } = "destek@tablewise.com.tr";
}
