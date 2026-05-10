namespace Tablewise.Domain.Enums;

/// <summary>
/// Bildirim tipi. Email/SMS template seçimi için kullanılır.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Rezervasyon onay bildirimi.
    /// </summary>
    Confirm = 0,

    /// <summary>
    /// Rezervasyon hatırlatma bildirimi (X saat önce).
    /// </summary>
    Reminder = 1,

    /// <summary>
    /// Rezervasyon iptal bildirimi.
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// No-show bildirimi (gelmedi).
    /// </summary>
    NoShow = 3,

    /// <summary>
    /// Hoşgeldin bildirimi (ilk rezervasyon).
    /// </summary>
    Welcome = 4,

    /// <summary>
    /// Şifre sıfırlama bildirimi.
    /// </summary>
    PasswordReset = 5,

    /// <summary>
    /// Rezervasyon değişiklik bildirimi.
    /// </summary>
    Modified = 6,

    /// <summary>
    /// Email doğrulama bildirimi.
    /// </summary>
    EmailVerification = 7,

    /// <summary>
    /// Personel davet bildirimi.
    /// </summary>
    StaffInvitation = 8,

    /// <summary>
    /// Deneme süresi bitiş bildirimi.
    /// </summary>
    TrialExpiry = 9,

    /// <summary>
    /// Plan yükseltme bildirimi.
    /// </summary>
    PlanUpgraded = 10,

    /// <summary>
    /// Plan ödeme başarısız bildirimi.
    /// </summary>
    PlanPaymentFailed = 11,

    /// <summary>
    /// Kapora ödeme bildirimi.
    /// </summary>
    DepositPaid = 12,

    /// <summary>
    /// Kapora iade bildirimi.
    /// </summary>
    DepositRefunded = 13,

    /// <summary>
    /// Yeni rezervasyon (venue sahibi için).
    /// </summary>
    NewReservationOwner = 14
}
