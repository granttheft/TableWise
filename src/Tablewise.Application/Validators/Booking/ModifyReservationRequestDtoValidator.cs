using FluentValidation;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Validators.Booking;

/// <summary>
/// ModifyReservationRequestDto için FluentValidation kuralları.
/// Rezervasyon değiştirme endpoint validasyonu.
/// </summary>
public sealed class ModifyReservationRequestDtoValidator : AbstractValidator<ModifyReservationRequestDto>
{
    private const int MinPartySize = 1;
    private const int MaxPartySize = 50;
    private const int MinReservationHoursAhead = 1;

    /// <summary>
    /// ModifyReservationRequestDtoValidator constructor.
    /// </summary>
    public ModifyReservationRequestDtoValidator()
    {
        RuleFor(x => x.NewDateTime)
            .GreaterThan(DateTime.UtcNow.AddHours(MinReservationHoursAhead))
            .WithMessage($"Yeni rezervasyon tarihi en az {MinReservationHoursAhead} saat sonrası için olmalıdır.")
            .When(x => x.NewDateTime.HasValue);

        RuleFor(x => x.NewPartySize)
            .InclusiveBetween(MinPartySize, MaxPartySize)
            .WithMessage($"Kişi sayısı {MinPartySize}-{MaxPartySize} arasında olmalıdır.")
            .When(x => x.NewPartySize.HasValue);

        RuleFor(x => x.NewTableId)
            .NotEqual(Guid.Empty).WithMessage("Geçersiz masa ID.")
            .When(x => x.NewTableId.HasValue);

        RuleFor(x => x)
            .Must(x => x.NewDateTime.HasValue || x.NewPartySize.HasValue || x.NewTableId.HasValue)
            .WithMessage("En az bir değişiklik yapılmalıdır (tarih, kişi sayısı veya masa).");
    }
}
