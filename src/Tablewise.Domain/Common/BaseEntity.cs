namespace Tablewise.Domain.Common;

/// <summary>
/// Tüm entity'ler için temel sınıf. ID, audit ve soft delete alanlarını içerir.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Entity'nin benzersiz kimliği (UUID).
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Entity'nin oluşturulma zamanı (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Entity'nin son güncellenme zamanı (UTC, nullable).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag. True ise entity silinmiş sayılır.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Soft delete işleminin gerçekleştiği zaman (UTC, nullable).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
