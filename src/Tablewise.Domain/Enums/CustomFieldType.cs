namespace Tablewise.Domain.Enums;

/// <summary>
/// Custom field (özel alan) tipi. VenueCustomField için kullanılır.
/// </summary>
public enum CustomFieldType
{
    /// <summary>
    /// Metin girişi (single line).
    /// </summary>
    Text = 0,

    /// <summary>
    /// Sayı girişi.
    /// </summary>
    Number = 1,

    /// <summary>
    /// Boolean (checkbox).
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// Seçim kutusu (dropdown). Options JSON'dan gelir.
    /// </summary>
    Select = 3,

    /// <summary>
    /// Tarih seçici.
    /// </summary>
    Date = 4
}
