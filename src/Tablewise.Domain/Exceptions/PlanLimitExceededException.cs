namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Plan limiti aşıldı. Upgrade gerekiyor. HTTP 402 veya 403 dönülür.
/// </summary>
public class PlanLimitExceededException : DomainException
{
    /// <summary>
    /// Limit tipi (örn: "MaxVenues", "MaxTables", "MaxRules").
    /// </summary>
    public string LimitType { get; }

    /// <summary>
    /// Mevcut limit değeri.
    /// </summary>
    public int CurrentLimit { get; }

    /// <summary>
    /// Upgrade URL (frontend route).
    /// </summary>
    public string UpgradeUrl { get; }

    /// <summary>
    /// PlanLimitExceededException constructor.
    /// </summary>
    /// <param name="limitType">Limit tipi</param>
    /// <param name="currentLimit">Mevcut limit</param>
    /// <param name="upgradeUrl">Upgrade sayfası URL</param>
    public PlanLimitExceededException(string limitType, int currentLimit, string upgradeUrl)
        : base($"You have reached the {limitType} limit ({currentLimit}) for your current plan. Please upgrade to continue.", "PLAN_LIMIT_EXCEEDED")
    {
        LimitType = limitType;
        CurrentLimit = currentLimit;
        UpgradeUrl = upgradeUrl;
    }

    /// <summary>
    /// PlanLimitExceededException constructor with custom message.
    /// </summary>
    /// <param name="limitType">Limit tipi</param>
    /// <param name="currentLimit">Mevcut limit</param>
    /// <param name="upgradeUrl">Upgrade sayfası URL</param>
    /// <param name="message">Özel hata mesajı</param>
    public PlanLimitExceededException(string limitType, int currentLimit, string upgradeUrl, string message)
        : base(message, "PLAN_LIMIT_EXCEEDED")
    {
        LimitType = limitType;
        CurrentLimit = currentLimit;
        UpgradeUrl = upgradeUrl;
    }
}
