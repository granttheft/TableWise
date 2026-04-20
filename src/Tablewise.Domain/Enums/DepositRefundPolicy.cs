namespace Tablewise.Domain.Enums;

/// <summary>
/// Kapora iade politikası. VenueSettings seviyesinde tanımlanır.
/// </summary>
public enum DepositRefundPolicy
{
    /// <summary>
    /// Tam iade. İptal edilirse tüm kapora iade edilir.
    /// </summary>
    FullRefund = 0,

    /// <summary>
    /// Kısmi iade. İptal süresi ve koşullara göre kısmi iade.
    /// </summary>
    PartialRefund = 1,

    /// <summary>
    /// İade yok. Hiçbir koşulda iade yapılmaz.
    /// </summary>
    NoRefund = 2
}
