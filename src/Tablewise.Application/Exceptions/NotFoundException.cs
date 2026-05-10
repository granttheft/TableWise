namespace Tablewise.Application.Exceptions;

/// <summary>
/// Entity bulunamadığında fırlatılan exception.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// NotFoundException constructor.
    /// </summary>
    /// <param name="entityName">Entity adı</param>
    /// <param name="key">Aranan key</param>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} ({key}) bulunamadı.")
    {
        EntityName = entityName;
        Key = key;
    }

    /// <summary>
    /// Entity adı
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Aranan key
    /// </summary>
    public object Key { get; }
}
