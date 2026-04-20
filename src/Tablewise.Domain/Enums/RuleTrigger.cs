namespace Tablewise.Domain.Enums;

/// <summary>
/// Kural tetikleyici. RuleTemplate hangi aşamada çalışacağını belirtir.
/// </summary>
public enum RuleTrigger
{
    /// <summary>
    /// Rezervasyon oluşturulurken.
    /// </summary>
    OnReservationCreate = 0,

    /// <summary>
    /// Masa atanırken.
    /// </summary>
    OnSeatAssign = 1,

    /// <summary>
    /// Rezervasyon iptal edilirken.
    /// </summary>
    OnCancel = 2
}
