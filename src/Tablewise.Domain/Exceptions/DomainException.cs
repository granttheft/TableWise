namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Domain katmanı için temel exception sınıfı.
/// Tüm domain exception'ları bundan türetilmelidir.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Hata kodu. API response'larda kullanılır.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// DomainException constructor.
    /// </summary>
    public DomainException()
    {
    }

    /// <summary>
    /// DomainException constructor with message.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// DomainException constructor with message and error code.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    public DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// DomainException constructor with message and inner exception.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="innerException">İç exception</param>
    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// DomainException constructor with message, error code and inner exception.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <param name="innerException">İç exception</param>
    public DomainException(string message, string errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
