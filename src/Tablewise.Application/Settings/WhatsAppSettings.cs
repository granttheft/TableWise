namespace Tablewise.Application.Settings;

/// <summary>
/// Twilio WhatsApp API yapılandırması.
/// Gerçek kimlik bilgileri appsettings.Local.json'da (git-ignored) tutulur.
/// </summary>
public sealed class WhatsAppSettings
{
    /// <summary>
    /// appsettings.json bölüm adı.
    /// </summary>
    public const string SectionName = "WhatsApp";

    /// <summary>
    /// Twilio Account SID.
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Twilio Auth Token.
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Gönderici WhatsApp numarası. Twilio Sandbox: "whatsapp:+14155238886"
    /// </summary>
    public string FromNumber { get; set; } = string.Empty;

    /// <summary>
    /// Twilio webhook imza doğrulama token'ı.
    /// </summary>
    public string WebhookVerifyToken { get; set; } = string.Empty;

    /// <summary>
    /// Sandbox modu aktif mi? True ise üretim Twilio numaraları yerine sandbox kullanılır.
    /// </summary>
    public bool SandboxMode { get; set; } = true;
}
