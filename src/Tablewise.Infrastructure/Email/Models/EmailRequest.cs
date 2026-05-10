namespace Tablewise.Infrastructure.Email.Models;

/// <summary>
/// Redis queue'da tutulacak email request modeli.
/// </summary>
public sealed class EmailRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? TenantId { get; set; }
    public Guid? ReservationId { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }
    public string TemplateName { get; set; } = string.Empty; // EmailTemplate enum name
    public string NotificationType { get; set; } = string.Empty; // NotificationType enum name
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; }
}
