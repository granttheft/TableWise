using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Tenant (Kiracı) entity. Multi-tenant sistemin temel birimi.
/// Her tenant bağımsız bir işletme/organizasyonu temsil eder.
/// BaseEntity'den türer (TenantScoped değil çünkü kendisi tenant'tır).
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Tenant adı (şirket/organizasyon adı).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug. Benzersiz olmalı.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Tenant email adresi (owner email). Benzersiz olmalı.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt ile hashlenmiş şifre.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Aktif plan ID'si (Plan tablosuna foreign key).
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Plan durumu (Trial, Active, PastDue, Suspended, Cancelled).
    /// </summary>
    public PlanStatus PlanStatus { get; set; }

    /// <summary>
    /// Deneme süresi bitiş tarihi (UTC).
    /// </summary>
    public DateTime? TrialEndsAt { get; set; }

    /// <summary>
    /// Plan yenileme tarihi (UTC). Bir sonraki ödeme tarihi.
    /// </summary>
    public DateTime? PlanRenewsAt { get; set; }

    /// <summary>
    /// Email doğrulandı mı?
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Email doğrulama token'ı (kullanılmadıysa null).
    /// </summary>
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// Şifre sıfırlama token'ı (kullanılmadıysa null).
    /// </summary>
    public string? PasswordResetToken { get; set; }

    /// <summary>
    /// Şifre sıfırlama token'ının son kullanma tarihi (UTC).
    /// </summary>
    public DateTime? PasswordResetExpiry { get; set; }

    /// <summary>
    /// Tenant özel ayarları (JSONB). Serbest metadata.
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// Bu ay yapılan rezervasyon sayısı. Plan limiti kontrolü için.
    /// Her ay başı sıfırlanır (background job).
    /// </summary>
    public int ReservationCountThisMonth { get; set; }

    /// <summary>
    /// Tenant aktif mi? False ise giriş yapamaz.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Tenant'a ait kullanıcılar (Owner + Staff).
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>
    /// Tenant'a ait mekanlar.
    /// </summary>
    public virtual ICollection<Venue> Venues { get; set; } = new List<Venue>();

    /// <summary>
    /// Tenant'ın abonelik geçmişi.
    /// </summary>
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    /// <summary>
    /// Tenant'ın aktif planı.
    /// </summary>
    public virtual Plan? Plan { get; set; }
}
