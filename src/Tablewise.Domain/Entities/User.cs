using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Kullanıcı entity. Owner veya Staff rolünde olabilir.
/// TenantScoped - her kullanıcı bir tenant'a aittir.
/// </summary>
public class User : TenantScopedEntity
{
    /// <summary>
    /// Kullanıcı email adresi. Tenant içinde benzersiz olmalı.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt ile hashlenmiş şifre.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcı adı.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcı soyadı.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcı rolü (Owner, Staff).
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Kullanıcı aktif mi? False ise giriş yapamaz.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Davet edilme tarihi (UTC). Davet ile gelen staff için dolu.
    /// </summary>
    public DateTime? InvitedAt { get; set; }

    /// <summary>
    /// Son giriş tarihi (UTC).
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Kullanıcı telefon numarası (opsiyonel).
    /// </summary>
    public string? PhoneNumber { get; set; }

    // Navigation Properties

    /// <summary>
    /// Kullanıcının ait olduğu tenant.
    /// </summary>
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Kullanıcının gönderdiği davetler.
    /// </summary>
    public virtual ICollection<UserInvitation> SentInvitations { get; set; } = new List<UserInvitation>();

    /// <summary>
    /// Kullanıcının gerçekleştirdiği audit log kayıtları.
    /// </summary>
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
