using FluentValidation;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Validators.Tenant;

/// <summary>
/// UpdateTenantDto için FluentValidation kuralları.
/// </summary>
public sealed class UpdateTenantDtoValidator : AbstractValidator<UpdateTenantDto>
{
    public UpdateTenantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İşletme adı zorunludur.")
            .MinimumLength(2).WithMessage("İşletme adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("İşletme adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Settings)
            .MaximumLength(5000).WithMessage("Settings alanı en fazla 5000 karakter olabilir.")
            .Must(BeValidJsonOrNull).WithMessage("Settings geçerli bir JSON formatında olmalıdır.")
            .When(x => !string.IsNullOrEmpty(x.Settings));
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
