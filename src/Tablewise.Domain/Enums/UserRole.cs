namespace Tablewise.Domain.Enums;

/// <summary>
/// Kullanıcı rol tipleri. Sistem genelinde yetkilendirme için kullanılır.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Süper admin (platform yöneticisi). Tüm tenant'lara erişebilir.
    /// </summary>
    SuperAdmin = 0,

    /// <summary>
    /// Tenant sahibi (Owner). Tenant içinde tüm yetkilere sahip.
    /// </summary>
    Owner = 1,

    /// <summary>
    /// Personel (Staff). Sınırlı yetkilere sahip, owner tarafından yetkilendirilebilir.
    /// </summary>
    Staff = 2
}
