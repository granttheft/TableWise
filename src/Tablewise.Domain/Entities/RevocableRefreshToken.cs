using Tablewise.Domain.Common;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Revoke edilebilir refresh token entity. JWT token rotation için kullanılır.
/// TenantScoped - her token bir tenant'a ve kullanıcıya aittir.
/// </summary>
public class RevocableRefreshToken : TenantScopedEntity
{
    /// <summary>
    /// Token sahibi kullanıcı ID'si.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Refresh token değeri (64-byte random → base64).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token'ın son kullanma tarihi (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token revoke edildi mi?
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Token revoke edilme tarihi (UTC, nullable).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Revoke eden kullanıcı/sistem bilgisi.
    /// </summary>
    public string? RevokedBy { get; set; }

    /// <summary>
    /// Bu token'ı replace eden yeni token (rotation için).
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Token'ın oluşturulduğu IP adresi.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Token'ın revoke edildiği IP adresi.
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// User agent bilgisi (cihaz/browser tespiti için).
    /// </summary>
    public string? UserAgent { get; set; }

    // Navigation Properties

    /// <summary>
    /// Token sahibi kullanıcı.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Token'ın ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    // Helper Properties

    /// <summary>
    /// Token geçerli mi? (Revoke edilmemiş ve süresi dolmamış)
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}
