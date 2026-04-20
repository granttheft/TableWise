namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Kullanıcı yetkisi yok (kimlik doğrulanmış ama yetki yok). HTTP 403 dönülür.
/// </summary>
public class ForbiddenException : DomainException
{
    /// <summary>
    /// Gereken rol veya yetki.
    /// </summary>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// ForbiddenException default constructor.
    /// </summary>
    public ForbiddenException()
        : base("You do not have permission to access this resource.", "FORBIDDEN")
    {
    }

    /// <summary>
    /// ForbiddenException constructor with custom message.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    public ForbiddenException(string message)
        : base(message, "FORBIDDEN")
    {
    }

    /// <summary>
    /// ForbiddenException constructor with required permission.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="requiredPermission">Gereken yetki</param>
    public ForbiddenException(string message, string requiredPermission)
        : base(message, "FORBIDDEN")
    {
        RequiredPermission = requiredPermission;
    }
}
