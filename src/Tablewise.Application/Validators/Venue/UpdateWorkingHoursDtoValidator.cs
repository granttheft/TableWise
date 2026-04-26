using FluentValidation;
using Tablewise.Application.DTOs.Venue;

namespace Tablewise.Application.Validators.Venue;

/// <summary>
/// UpdateWorkingHoursDto için FluentValidation kuralları.
/// </summary>
public sealed class UpdateWorkingHoursDtoValidator : AbstractValidator<UpdateWorkingHoursDto>
{
    public UpdateWorkingHoursDtoValidator()
    {
        RuleFor(x => x.WorkingHours)
            .NotEmpty().WithMessage("Çalışma saatleri zorunludur.")
            .MaximumLength(2000).WithMessage("Çalışma saatleri en fazla 2000 karakter olabilir.")
            .Must(BeValidJson).WithMessage("Çalışma saatleri geçerli bir JSON formatında olmalıdır.");
    }

    private bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
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
