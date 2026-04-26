using FluentValidation;
using Tablewise.Application.DTOs.Table;

namespace Tablewise.Application.Validators.Table;

/// <summary>
/// CreateTableDto için FluentValidation kuralları.
/// </summary>
public sealed class CreateTableDtoValidator : AbstractValidator<CreateTableDto>
{
    public CreateTableDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Masa adı zorunludur.")
            .MinimumLength(1).WithMessage("Masa adı en az 1 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("Masa adı en fazla 50 karakter olabilir.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(1).WithMessage("Kapasite en az 1 olmalıdır.")
            .LessThanOrEqualTo(50).WithMessage("Kapasite en fazla 50 olabilir.");

        RuleFor(x => x.Location)
            .IsInEnum().WithMessage("Geçerli bir lokasyon seçiniz.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
