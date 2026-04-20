namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Validation hatası. Birden fazla alan hatası içerebilir. HTTP 400 dönülür.
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Alan adı ve hata mesajı eşleşmeleri.
    /// Key: Alan adı (örn: "Email", "PhoneNumber")
    /// Value: Hata mesajı listesi
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// ValidationException constructor with errors dictionary.
    /// </summary>
    /// <param name="errors">Alan hataları dictionary</param>
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    /// <summary>
    /// ValidationException constructor with single field error.
    /// </summary>
    /// <param name="fieldName">Alan adı</param>
    /// <param name="errorMessage">Hata mesajı</param>
    public ValidationException(string fieldName, string errorMessage)
        : base($"Validation failed for field '{fieldName}': {errorMessage}", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]>
        {
            { fieldName, new[] { errorMessage } }
        };
    }

    /// <summary>
    /// ValidationException constructor with custom message and errors.
    /// </summary>
    /// <param name="message">Hata mesajı</param>
    /// <param name="errors">Alan hataları dictionary</param>
    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base(message, "VALIDATION_ERROR")
    {
        Errors = errors;
    }
}
