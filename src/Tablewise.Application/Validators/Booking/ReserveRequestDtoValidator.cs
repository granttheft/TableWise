using FluentValidation;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Validators.Booking;

/// <summary>
/// ReserveRequestDto için FluentValidation kuralları.
/// Public booking endpoint validasyonu.
/// </summary>
public sealed class ReserveRequestDtoValidator : AbstractValidator<ReserveRequestDto>
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;
    private const int MinPartySize = 1;
    private const int MaxPartySize = 50;
    private const int MaxSpecialRequestsLength = 500;
    private const int MinReservationHoursAhead = 1;

    /// <summary>
    /// ReserveRequestDtoValidator constructor.
    /// </summary>
    public ReserveRequestDtoValidator()
    {
        RuleFor(x => x.GuestName)
            .NotEmpty().WithMessage("Misafir adı zorunludur.")
            .MinimumLength(MinNameLength).WithMessage($"Misafir adı en az {MinNameLength} karakter olmalıdır.")
            .MaximumLength(MaxNameLength).WithMessage($"Misafir adı en fazla {MaxNameLength} karakter olabilir.");

        RuleFor(x => x.GuestPhone)
            .NotEmpty().WithMessage("Telefon numarası zorunludur.")
            .Matches(@"^(\+90|0)?[0-9]{10,15}$").WithMessage("Geçerli bir telefon numarası giriniz.");

        RuleFor(x => x.GuestEmail)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(255).WithMessage("Email en fazla 255 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.GuestEmail));

        RuleFor(x => x.PartySize)
            .InclusiveBetween(MinPartySize, MaxPartySize)
            .WithMessage($"Kişi sayısı {MinPartySize}-{MaxPartySize} arasında olmalıdır.");

        RuleFor(x => x.ReservedFor)
            .NotEmpty().WithMessage("Rezervasyon tarihi zorunludur.")
            .GreaterThan(DateTime.UtcNow.AddHours(MinReservationHoursAhead))
            .WithMessage($"Rezervasyon en az {MinReservationHoursAhead} saat sonrası için yapılabilir.");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(MaxSpecialRequestsLength)
            .WithMessage($"Özel istekler en fazla {MaxSpecialRequestsLength} karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.SpecialRequests));

        RuleFor(x => x.PrivacyPolicyAccepted)
            .Equal(true).WithMessage("KVKK aydınlatma metnini kabul etmelisiniz.");

        RuleFor(x => x.TableId)
            .NotEqual(Guid.Empty).WithMessage("Geçersiz masa ID.")
            .When(x => x.TableId.HasValue);

        RuleFor(x => x.TableCombinationId)
            .NotEqual(Guid.Empty).WithMessage("Geçersiz masa birleşimi ID.")
            .When(x => x.TableCombinationId.HasValue);

        RuleFor(x => x)
            .Must(x => !(x.TableId.HasValue && x.TableCombinationId.HasValue))
            .WithMessage("Aynı anda hem masa hem de masa birleşimi seçilemez.");
    }
}
