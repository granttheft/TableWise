namespace Tablewise.Domain.Exceptions;

/// <summary>
/// İş kuralı ihlali. Domain logic tarafından fırlatılır. HTTP 400 veya 409 dönülür.
/// </summary>
public class BusinessRuleException : DomainException
{
    /// <summary>
    /// İhlal edilen kural adı veya kodu.
    /// </summary>
    public string? RuleName { get; set; }

    /// <summary>
    /// BusinessRuleException constructor.
    /// </summary>
    /// <param name="message">İş kuralı ihlal mesajı</param>
    public BusinessRuleException(string message)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
    }

    /// <summary>
    /// BusinessRuleException constructor with rule name.
    /// </summary>
    /// <param name="message">İş kuralı ihlal mesajı</param>
    /// <param name="ruleName">Kural adı</param>
    public BusinessRuleException(string message, string ruleName)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
    }

    /// <summary>
    /// BusinessRuleException constructor with rule name and inner exception.
    /// </summary>
    /// <param name="message">İş kuralı ihlal mesajı</param>
    /// <param name="ruleName">Kural adı</param>
    /// <param name="innerException">İç exception</param>
    public BusinessRuleException(string message, string ruleName, Exception innerException)
        : base(message, "BUSINESS_RULE_VIOLATION", innerException)
    {
        RuleName = ruleName;
    }
}
