using FluentValidation;
using Tablewise.Application.DTOs.VenueClosure;

namespace Tablewise.Application.Validators.VenueClosure;

/// <summary>
/// CreateVenueClosureDto için FluentValidation kuralları.
/// </summary>
public sealed class CreateVenueClosureDtoValidator : AbstractValidator<CreateVenueClosureDto>
{
    public CreateVenueClosureDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden küçük olamaz.");

        RuleFor(x => x.OpenTime)
            .NotEmpty().WithMessage("Açılış saati zorunludur.")
            .Must(BeValidTimeFormat).WithMessage("Geçerli bir saat formatı giriniz (HH:mm).")
            .When(x => !x.IsFullDay);

        RuleFor(x => x.CloseTime)
            .NotEmpty().WithMessage("Kapanış saati zorunludur.")
            .Must(BeValidTimeFormat).WithMessage("Geçerli bir saat formatı giriniz (HH:mm).")
            .When(x => !x.IsFullDay);

        RuleFor(x => x)
            .Must(x => ValidateTimeRange(x.OpenTime, x.CloseTime))
            .WithMessage("Kapanış saati açılış saatinden büyük olmalıdır.")
            .When(x => !x.IsFullDay && !string.IsNullOrEmpty(x.OpenTime) && !string.IsNullOrEmpty(x.CloseTime));

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Neden en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }

    private bool BeValidTimeFormat(string? time)
    {
        if (string.IsNullOrEmpty(time))
            return false;

        return TimeSpan.TryParseExact(time, @"hh\:mm", null, out _);
    }

    private bool ValidateTimeRange(string? openTime, string? closeTime)
    {
        if (string.IsNullOrEmpty(openTime) || string.IsNullOrEmpty(closeTime))
            return true;

        if (!TimeSpan.TryParseExact(openTime, @"hh\:mm", null, out var open))
            return true;

        if (!TimeSpan.TryParseExact(closeTime, @"hh\:mm", null, out var close))
            return true;

        return close > open;
    }
}
