namespace Tablewise.Domain.Common;

/// <summary>
/// Multi-tenant sistemde tenant izolasyonu gerektiren entity'ler için temel sınıf.
/// BaseEntity'den türer ve TenantId alanı ekler. EF Core Global Query Filter ile otomatik filtrelenir.
/// </summary>
public abstract class TenantScopedEntity : BaseEntity
{
    /// <summary>
    /// Entity'nin ait olduğu tenant'ın benzersiz kimliği (UUID).
    /// Her kayıt mutlaka bir tenant'a aittir. Indexed olmalıdır.
    /// </summary>
    public Guid TenantId { get; set; }
}
