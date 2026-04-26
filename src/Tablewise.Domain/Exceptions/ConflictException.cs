namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Çakışma hatası. Kaynak zaten mevcut veya işlem başka bir işlemle çakışıyor.
/// HTTP 409 Conflict döndürülür.
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Çakışan kaynak adı.
    /// </summary>
    public string? ResourceName { get; }

    /// <summary>
    /// Çakışan kaynak ID'si.
    /// </summary>
    public string? ResourceId { get; }

    /// <summary>
    /// Hata kodu.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// ConflictException constructor.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu (default: CONFLICT)</param>
    public ConflictException(string message, string errorCode = "CONFLICT")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// ConflictException constructor with resource info.
    /// </summary>
    /// <param name="resourceName">Çakışan kaynak adı</param>
    /// <param name="resourceId">Çakışan kaynak ID'si</param>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    public ConflictException(string resourceName, string resourceId, string message, string errorCode = "CONFLICT")
        : base(message)
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
        ErrorCode = errorCode;
    }
}
