using System.Reflection;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Tablewise.Domain.Enums;

namespace Tablewise.Infrastructure.Email.Services;

/// <summary>
/// Email şablonlarını render eder. Embedded resources'dan HTML okur, placeholder replace eder, plain text üretir.
/// </summary>
public sealed class EmailTemplateRenderer
{
    private static readonly Assembly Assembly = typeof(EmailTemplateRenderer).Assembly;
    private static readonly Regex PlaceholderRegex = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    /// <summary>
    /// Şablonu render eder.
    /// </summary>
    public (string Html, string PlainText) Render(EmailTemplate template, Dictionary<string, object> data)
    {
        var templateName = GetTemplateName(template);
        var html = LoadTemplate(templateName);
        html = ReplacePlaceholders(html, data);
        var plainText = GeneratePlainText(html);
        return (html, plainText);
    }

    private static string GetTemplateName(EmailTemplate template) => template switch
    {
        EmailTemplate.Welcome => "welcome.html",
        EmailTemplate.EmailVerification => "email-verification.html",
        EmailTemplate.PasswordReset => "password-reset.html",
        EmailTemplate.ReservationConfirm => "reservation-confirm.html",
        EmailTemplate.ReservationModified => "reservation-modified.html",
        EmailTemplate.ReservationCancelled => "reservation-cancelled.html",
        EmailTemplate.ReservationReminder => "reservation-reminder.html",
        EmailTemplate.NewReservationOwner => "new-reservation-owner.html",
        EmailTemplate.NoShowNotification => "no-show-notification.html",
        EmailTemplate.StaffInvitation => "staff-invitation.html",
        EmailTemplate.TrialExpiryReminder => "trial-expiry-reminder.html",
        EmailTemplate.PlanUpgraded => "plan-upgraded.html",
        EmailTemplate.PlanPaymentFailed => "plan-payment-failed.html",
        EmailTemplate.DepositPaid => "deposit-paid.html",
        EmailTemplate.DepositRefunded => "deposit-refunded.html",
        _ => throw new ArgumentException($"Unknown template: {template}")
    };

    private static string LoadTemplate(string name)
    {
        var resourceName = $"Tablewise.Infrastructure.Email.Templates.{name}";
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Template not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string ReplacePlaceholders(string html, Dictionary<string, object> data)
    {
        return PlaceholderRegex.Replace(html, match =>
        {
            var key = match.Groups[1].Value;
            return data.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : match.Value;
        });
    }

    private static string GeneratePlainText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.InnerText.Trim();
    }
}
