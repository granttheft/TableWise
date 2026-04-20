namespace Tablewise.Domain.Common;

/// <summary>
/// Generic Result pattern. CQRS handler'lar başarılı/başarısız durumları bu sınıf ile döner.
/// Exception fırlatmadan hata yönetimi sağlar.
/// </summary>
/// <typeparam name="T">Result değer tipi</typeparam>
public class Result<T>
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// İşlem başarısızsa hata mesajı.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// İşlem başarısızsa hata kodu.
    /// </summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// İşlem başarılıysa sonuç değeri.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// İşlem başarısız mı?
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Başarılı result oluşturur.
    /// </summary>
    /// <param name="value">Sonuç değeri</param>
    /// <returns>Başarılı Result instance</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, null);
    }

    /// <summary>
    /// Başarısız result oluşturur.
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <returns>Başarısız Result instance</returns>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error, null);
    }

    /// <summary>
    /// Başarısız result oluşturur (hata kodu ile).
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Başarısız Result instance</returns>
    public static Result<T> Failure(string error, string errorCode)
    {
        return new Result<T>(false, default, error, errorCode);
    }

    /// <summary>
    /// Result değerini başka bir tipe map eder.
    /// </summary>
    /// <typeparam name="TNew">Yeni tip</typeparam>
    /// <param name="mapper">Mapper fonksiyonu</param>
    /// <returns>Yeni Result instance</returns>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsSuccess && Value != null)
        {
            return Result<TNew>.Success(mapper(Value));
        }

        return Result<TNew>.Failure(Error ?? "Unknown error", ErrorCode ?? "UNKNOWN_ERROR");
    }

    /// <summary>
    /// Result başarısızsa bir action çalıştırır.
    /// </summary>
    /// <param name="action">Çalıştırılacak action</param>
    /// <returns>Kendisi (chaining için)</returns>
    public Result<T> OnFailure(Action<string?, string?> action)
    {
        if (IsFailure)
        {
            action(Error, ErrorCode);
        }

        return this;
    }

    /// <summary>
    /// Result başarılıysa bir action çalıştırır.
    /// </summary>
    /// <param name="action">Çalıştırılacak action</param>
    /// <returns>Kendisi (chaining için)</returns>
    public Result<T> OnSuccess(Action<T?> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }
}

/// <summary>
/// Non-generic Result pattern. Değer döndürmeyen işlemler için.
/// </summary>
public class Result
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// İşlem başarısızsa hata mesajı.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// İşlem başarısızsa hata kodu.
    /// </summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// İşlem başarısız mı?
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Başarılı result oluşturur.
    /// </summary>
    /// <returns>Başarılı Result instance</returns>
    public static Result Success()
    {
        return new Result(true, null, null);
    }

    /// <summary>
    /// Başarısız result oluşturur.
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <returns>Başarısız Result instance</returns>
    public static Result Failure(string error)
    {
        return new Result(false, error, null);
    }

    /// <summary>
    /// Başarısız result oluşturur (hata kodu ile).
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Başarısız Result instance</returns>
    public static Result Failure(string error, string errorCode)
    {
        return new Result(false, error, errorCode);
    }

    /// <summary>
    /// Result başarısızsa bir action çalıştırır.
    /// </summary>
    /// <param name="action">Çalıştırılacak action</param>
    /// <returns>Kendisi (chaining için)</returns>
    public Result OnFailure(Action<string?, string?> action)
    {
        if (IsFailure)
        {
            action(Error, ErrorCode);
        }

        return this;
    }

    /// <summary>
    /// Result başarılıysa bir action çalıştırır.
    /// </summary>
    /// <param name="action">Çalıştırılacak action</param>
    /// <returns>Kendisi (chaining için)</returns>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            action();
        }

        return this;
    }
}
