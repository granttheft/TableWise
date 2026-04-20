namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Kullanıcı kimlik doğrulaması yapılmamış. HTTP 401 dönülür.
/// </summary>
public class UnauthorizedException : DomainException
{
    /// <summary>
    /// UnauthorizedException default constructor.
    /// </summary>
    public UnauthorizedException()
        : base("Authentication is required to access this resource.", "UNAUTHORIZED")
    {
    }

    /// <summary>
    /// UnauthorizedException constructor with custom message.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    public UnauthorizedException(string message)
        : base(message, "UNAUTHORIZED")
    {
    }

    /// <summary>
    /// UnauthorizedException constructor with message and inner exception.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="innerException">İç exception</param>
    public UnauthorizedException(string message, Exception innerException)
        : base(message, "UNAUTHORIZED", innerException)
    {
    }
}
