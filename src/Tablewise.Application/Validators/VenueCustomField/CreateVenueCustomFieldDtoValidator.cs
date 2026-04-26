using FluentValidation;
using Tablewise.Application.DTOs.VenueCustomField;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Validators.VenueCustomField;

/// <summary>
/// CreateVenueCustomFieldDto için FluentValidation kuralları.
/// </summary>
public sealed class CreateVenueCustomFieldDtoValidator : AbstractValidator<CreateVenueCustomFieldDto>
{
    public CreateVenueCustomFieldDtoValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Alan etiketi zorunludur.")
            .MinimumLength(2).WithMessage("Alan etiketi en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Alan etiketi en fazla 100 karakter olabilir.");

        RuleFor(x => x.FieldType)
            .IsInEnum().WithMessage("Geçerli bir alan tipi seçiniz.");

        RuleFor(x => x.Options)
            .NotEmpty().WithMessage("Select tipi için seçenekler zorunludur.")
            .Must(BeValidJson).WithMessage("Seçenekler geçerli bir JSON array formatında olmalıdır.")
            .When(x => x.FieldType == CustomFieldType.Select);
    }

    private bool BeValidJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return false;

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
