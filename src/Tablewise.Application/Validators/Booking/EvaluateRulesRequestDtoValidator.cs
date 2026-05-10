using FluentValidation;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Validators.Booking;

/// <summary>
/// EvaluateRulesRequestDto için FluentValidation kuralları.
/// Kural ön izleme endpoint validasyonu.
/// </summary>
public sealed class EvaluateRulesRequestDtoValidator : AbstractValidator<EvaluateRulesRequestDto>
{
    private const int MinPartySize = 1;
    private const int MaxPartySize = 50;

    /// <summary>
    /// EvaluateRulesRequestDtoValidator constructor.
    /// </summary>
    public EvaluateRulesRequestDtoValidator()
    {
        RuleFor(x => x.PartySize)
            .InclusiveBetween(MinPartySize, MaxPartySize)
            .WithMessage($"Kişi sayısı {MinPartySize}-{MaxPartySize} arasında olmalıdır.");

        RuleFor(x => x.ReservedFor)
            .NotEmpty().WithMessage("Rezervasyon tarihi zorunludur.")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Rezervasyon tarihi geçmiş olamaz.");

        RuleFor(x => x.TableId)
            .NotEqual(Guid.Empty).WithMessage("Geçersiz masa ID.")
            .When(x => x.TableId.HasValue);

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail));

        RuleFor(x => x.CustomerPhone)
            .Matches(@"^(\+90|0)?[0-9]{10,15}$").WithMessage("Geçerli bir telefon numarası giriniz.")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone));
    }
}
