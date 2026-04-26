using FluentValidation;
using Tablewise.Application.DTOs.Venue;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Validators.Venue;

/// <summary>
/// CreateVenueDto için FluentValidation kuralları.
/// </summary>
public sealed class CreateVenueDtoValidator : AbstractValidator<CreateVenueDto>
{
    public CreateVenueDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Mekan adı zorunludur.")
            .MinimumLength(2).WithMessage("Mekan adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Mekan adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("Saat dilimi zorunludur.")
            .Must(BeValidTimeZone).WithMessage("Geçerli bir saat dilimi giriniz.");

        RuleFor(x => x.SlotDurationMinutes)
            .GreaterThanOrEqualTo(15).WithMessage("Slot süresi en az 15 dakika olmalıdır.")
            .LessThanOrEqualTo(480).WithMessage("Slot süresi en fazla 480 dakika (8 saat) olabilir.");

        RuleFor(x => x.DepositAmount)
            .GreaterThan(0).WithMessage("Kapora tutarı 0'dan büyük olmalıdır.")
            .When(x => x.DepositEnabled && x.DepositAmount.HasValue);

        RuleFor(x => x.DepositRefundHours)
            .GreaterThanOrEqualTo(0).WithMessage("İade için minimum saat 0 veya daha büyük olmalıdır.")
            .LessThanOrEqualTo(168).WithMessage("İade için minimum saat en fazla 168 saat (7 gün) olabilir.")
            .When(x => x.DepositEnabled && x.DepositRefundHours.HasValue);

        RuleFor(x => x.DepositPartialPercent)
            .GreaterThanOrEqualTo(0).WithMessage("Kısmi iade yüzdesi 0 ile 100 arası olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Kısmi iade yüzdesi 0 ile 100 arası olmalıdır.")
            .When(x => x.DepositEnabled 
                && x.DepositRefundPolicy == DepositRefundPolicy.PartialRefund 
                && x.DepositPartialPercent.HasValue);

        RuleFor(x => x.WorkingHours)
            .MaximumLength(2000).WithMessage("Çalışma saatleri en fazla 2000 karakter olabilir.")
            .Must(BeValidJsonOrNull).WithMessage("Çalışma saatleri geçerli bir JSON formatında olmalıdır.")
            .When(x => !string.IsNullOrEmpty(x.WorkingHours));
    }

    private bool BeValidTimeZone(string timeZone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool BeValidJsonOrNull(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
