using FluentValidation;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Validators.Rule;

/// <summary>
/// TestRuleRequestDto için FluentValidation kuralları.
/// </summary>
public sealed class TestRuleRequestDtoValidator : AbstractValidator<TestRuleRequestDto>
{
    private const int MinPartySize = 1;
    private const int MaxPartySize = 50;

    /// <summary>
    /// TestRuleRequestDtoValidator constructor.
    /// </summary>
    public TestRuleRequestDtoValidator()
    {
        RuleFor(x => x.PartySize)
            .InclusiveBetween(MinPartySize, MaxPartySize)
            .WithMessage($"Kişi sayısı {MinPartySize}-{MaxPartySize} arasında olmalıdır.");

        RuleFor(x => x.ReservedFor)
            .NotEmpty().WithMessage("Rezervasyon tarihi zorunludur.")
            .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
            .WithMessage("Rezervasyon tarihi geçmiş olamaz.");

        RuleFor(x => x.TableId)
            .NotEqual(Guid.Empty).WithMessage("Geçersiz masa ID.")
            .When(x => x.TableId.HasValue);

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(255).WithMessage("Email en fazla 255 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail));

        RuleFor(x => x.CustomerPhone)
            .Matches(@"^(\+90|0)?[0-9]{10,15}$").WithMessage("Geçerli bir telefon numarası giriniz.")
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone));
    }
}
