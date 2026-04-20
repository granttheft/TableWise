using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Kullanıcı davet entity. Owner tarafından staff davet edildiğinde oluşturulur.
/// Token ile davet kabul edilir ve User oluşturulur.
/// </summary>
public class UserInvitation : TenantScopedEntity
{
    /// <summary>
    /// Davet edilen kişinin email adresi.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Davet edilen kişiye verilecek rol (Staff).
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Davet token'ı. Benzersiz olmalı. URL'de kullanılır.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Davet son kullanma tarihi (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Davet kabul edilme tarihi (UTC). Null ise henüz kabul edilmemiş.
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Daveti gönderen kullanıcının ID'si.
    /// </summary>
    public Guid InvitedByUserId { get; set; }

    // Navigation Properties

    /// <summary>
    /// Daveti gönderen kullanıcı.
    /// </summary>
    public virtual User? InvitedBy { get; set; }

    /// <summary>
    /// Davet edilen kullanıcının ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }
}
