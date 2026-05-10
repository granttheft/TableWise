using Tablewise.Domain.Enums;

namespace Tablewise.Application.Interfaces;

/// <summary>
/// Email gönderim servisi interface.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Hoşgeldin emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="userName">Kullanıcı adı</param>
    /// <param name="verificationLink">Email doğrulama linki</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Şifre sıfırlama emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="userName">Kullanıcı adı</param>
    /// <param name="resetLink">Şifre sıfırlama linki</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Email doğrulama başarılı emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="userName">Kullanıcı adı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendEmailVerifiedNotificationAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Personel davet emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="tenantName">İşletme adı</param>
    /// <param name="inviterName">Davet eden kişi</param>
    /// <param name="role">Atanacak rol</param>
    /// <param name="inviteLink">Davet linki</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendStaffInvitationEmailAsync(
        string toEmail,
        string tenantName,
        string inviterName,
        string role,
        string inviteLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rezervasyon onay emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="reservedFor">Rezervasyon tarihi/saati</param>
    /// <param name="confirmCode">Onay kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendReservationConfirmationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rezervasyon iptal emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="reservedFor">Rezervasyon tarihi/saati</param>
    /// <param name="confirmCode">Onay kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendReservationCancellationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rezervasyon değişiklik emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="oldDateTime">Eski tarih/saat</param>
    /// <param name="newDateTime">Yeni tarih/saat</param>
    /// <param name="newConfirmCode">Yeni onay kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendReservationModificationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime oldDateTime,
        DateTime newDateTime,
        string newConfirmCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rezervasyon hatırlatma emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="venueAddress">Mekan adresi</param>
    /// <param name="reservedFor">Rezervasyon tarihi/saati</param>
    /// <param name="confirmCode">Onay kodu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendReservationReminderAsync(
        string toEmail,
        string guestName,
        string venueName,
        string? venueAddress,
        DateTime reservedFor,
        string confirmCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ham email gönderir (HTML + plain text).
    /// </summary>
    /// <param name="to">Alıcı email</param>
    /// <param name="subject">Konu</param>
    /// <param name="htmlBody">HTML içerik</param>
    /// <param name="plainTextBody">Plain text içerik (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Şablon kullanarak email gönderir.
    /// </summary>
    /// <param name="to">Alıcı email</param>
    /// <param name="template">Email şablonu</param>
    /// <param name="data">Şablon verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendTemplatedAsync(
        string to,
        EmailTemplate template,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deneme süresi bitiş hatırlatma emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="tenantName">Tenant adı</param>
    /// <param name="daysLeft">Kalan gün sayısı</param>
    /// <param name="upgradeUrl">Yükseltme URL'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendTrialExpiryReminderAsync(
        string toEmail,
        string tenantName,
        int daysLeft,
        string upgradeUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni rezervasyon bildirimi emaili gönderir (venue sahibine).
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="ownerName">Venue sahibi adı</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="reservedFor">Rezervasyon tarihi/saati</param>
    /// <param name="partySize">Kişi sayısı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="adminUrl">Admin panel URL'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendNewReservationToOwnerAsync(
        string toEmail,
        string ownerName,
        string guestName,
        DateTime reservedFor,
        int partySize,
        string venueName,
        string adminUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// No-show bildirimi emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="reservedFor">Rezervasyon tarihi/saati</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendNoShowNotificationAsync(
        string toEmail,
        string guestName,
        string venueName,
        DateTime reservedFor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Plan yükseltme bildirimi emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="tenantName">Tenant adı</param>
    /// <param name="newPlan">Yeni plan adı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendPlanUpgradedAsync(
        string toEmail,
        string tenantName,
        string newPlan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Plan ödeme başarısız bildirimi emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="tenantName">Tenant adı</param>
    /// <param name="planName">Plan adı</param>
    /// <param name="dueDate">Son ödeme tarihi</param>
    /// <param name="paymentUrl">Ödeme URL'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendPlanPaymentFailedAsync(
        string toEmail,
        string tenantName,
        string planName,
        DateTime dueDate,
        string paymentUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kapora ödeme bildirimi emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="amount">Kapora tutarı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="reservedFor">Rezervasyon tarihi/saati</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendDepositPaidAsync(
        string toEmail,
        string guestName,
        decimal amount,
        string venueName,
        DateTime reservedFor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kapora iade bildirimi emaili gönderir.
    /// </summary>
    /// <param name="toEmail">Alıcı email</param>
    /// <param name="guestName">Misafir adı</param>
    /// <param name="amount">İade tutarı</param>
    /// <param name="venueName">Mekan adı</param>
    /// <param name="reason">İade nedeni</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task SendDepositRefundedAsync(
        string toEmail,
        string guestName,
        decimal amount,
        string venueName,
        string reason,
        CancellationToken cancellationToken = default);
}
