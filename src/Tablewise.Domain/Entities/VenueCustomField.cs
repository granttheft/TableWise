using Tablewise.Domain.Common;
using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Entities;

/// <summary>
/// Mekan özel alan (custom field) entity. Rezervasyon formunda ekstra bilgi toplamak için.
/// Örn: "Doğum günü mü?", "Kek isteği var mı?", "Allerji bilgisi" vb.
/// </summary>
public class VenueCustomField : TenantScopedEntity
{
    /// <summary>
    /// Custom field hangi mekana ait (Foreign Key).
    /// </summary>
    public Guid VenueId { get; set; }

    /// <summary>
    /// Alan adı (Name). Internal identifier.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Alan etiketi (label). Örn: "Özel İstek", "Allerji Bilgisi".
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Placeholder metin (input alanında gösterilir).
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Alan tipi (Text, Number, Boolean, Select, Date).
    /// </summary>
    public CustomFieldType FieldType { get; set; }

    /// <summary>
    /// Alan zorunlu mu?
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Sıralama önceliği. Formda gösterim sırası için.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Select tipi için seçenekler (JSONB). Format: ["Seçenek 1", "Seçenek 2"].
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// Public rezervasyon formunda gösterilsin mi?
    /// False ise sadece staff tarafından görülür.
    /// </summary>
    public bool IsPublic { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Custom field'ın ait olduğu mekan.
    /// </summary>
    public virtual Venue? Venue { get; set; }
}
