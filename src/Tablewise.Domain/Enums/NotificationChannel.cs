namespace Tablewise.Domain.Enums;

/// <summary>
/// Bildirim kanalı. NotificationLog için kullanılır.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Email bildirimi (tüm planlarda).
    /// </summary>
    Email = 0,

    /// <summary>
    /// SMS bildirimi (Pro+ planlarda).
    /// </summary>
    Sms = 1,

    /// <summary>
    /// Push bildirimi (gelecek özellik).
    /// </summary>
    Push = 2
}
