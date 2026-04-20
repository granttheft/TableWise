namespace Tablewise.Domain.Enums;

/// <summary>
/// Masa lokasyon tipleri. Filtre ve arama için kullanılır.
/// </summary>
public enum TableLocation
{
    /// <summary>
    /// İç mekan.
    /// </summary>
    Indoor = 0,

    /// <summary>
    /// Açık alan (outdoor).
    /// </summary>
    Outdoor = 1,

    /// <summary>
    /// Balkon.
    /// </summary>
    Balcony = 2,

    /// <summary>
    /// Bar alanı.
    /// </summary>
    Bar = 3,

    /// <summary>
    /// Özel oda (private).
    /// </summary>
    Private = 4,

    /// <summary>
    /// Teras.
    /// </summary>
    Terrace = 5,

    /// <summary>
    /// Bahçe.
    /// </summary>
    Garden = 6
}
