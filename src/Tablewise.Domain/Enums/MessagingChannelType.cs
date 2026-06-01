namespace Tablewise.Domain.Enums;

/// <summary>
/// Mesajlaşma kanalı türü.
/// </summary>
public enum MessagingChannelType
{
    /// <summary>
    /// WhatsApp Business API.
    /// </summary>
    WhatsApp = 0,

    /// <summary>
    /// SMS (ileride eklenecek).
    /// </summary>
    Sms = 1,
}
