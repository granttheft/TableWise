using FluentValidation;
using Tablewise.Application.DTOs.TableCombination;

namespace Tablewise.Application.Validators.TableCombination;

/// <summary>
/// UpdateTableCombinationDto için FluentValidation kuralları.
/// </summary>
public sealed class UpdateTableCombinationDtoValidator : AbstractValidator<UpdateTableCombinationDto>
{
    public UpdateTableCombinationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kombinasyon adı zorunludur.")
            .MinimumLength(2).WithMessage("Kombinasyon adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Kombinasyon adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.TableIds)
            .NotEmpty().WithMessage("En az 2 masa seçilmelidir.")
            .Must(x => x.Count >= 2).WithMessage("En az 2 masa seçilmelidir.")
            .Must(x => x.Count <= 10).WithMessage("En fazla 10 masa birleştirilebilir.");

        RuleFor(x => x.CombinedCapacity)
            .GreaterThan(0).WithMessage("Birleşik kapasite 0'dan büyük olmalıdır.");
    }
}
