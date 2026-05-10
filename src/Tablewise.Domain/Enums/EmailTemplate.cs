namespace Tablewise.Domain.Enums;

/// <summary>
/// Email şablon tipleri. EmailTemplateRenderer ile kullanılır.
/// </summary>
public enum EmailTemplate
{
    Welcome = 0,
    EmailVerification = 1,
    PasswordReset = 2,
    ReservationConfirm = 3,
    ReservationModified = 4,
    ReservationCancelled = 5,
    ReservationReminder = 6,
    NewReservationOwner = 7,
    NoShowNotification = 8,
    StaffInvitation = 9,
    TrialExpiryReminder = 10,
    PlanUpgraded = 11,
    PlanPaymentFailed = 12,
    DepositPaid = 13,
    DepositRefunded = 14
}
