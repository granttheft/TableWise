namespace Tablewise.Domain.Exceptions;

/// <summary>
/// Entity bulunamadığında fırlatılır. HTTP 404 dönülür.
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// Entity adı (örn: "Reservation", "Table").
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Aranan entity ID'si.
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// NotFoundException constructor.
    /// </summary>
    /// <param name="entityName">Entity adı</param>
    /// <param name="entityId">Entity ID</param>
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID '{entityId}' was not found.", "NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    /// <summary>
    /// NotFoundException constructor with custom message.
    /// </summary>
    /// <param name="entityName">Entity adı</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="message">Özel hata mesajı</param>
    public NotFoundException(string entityName, object entityId, string message)
        : base(message, "NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
