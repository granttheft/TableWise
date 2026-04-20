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
    PasswordReset = 5
}
