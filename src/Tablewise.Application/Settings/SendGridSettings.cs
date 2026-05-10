namespace Tablewise.Application.Settings;

/// <summary>
/// SendGrid email servisi ayarları.
/// </summary>
public sealed class SendGridSettings
{
    public const string SectionName = "SendGrid";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@tablewise.com.tr";
    public string FromName { get; set; } = "Tablewise";
    public string ReplyTo { get; set; } = "destek@tablewise.com.tr";
}
