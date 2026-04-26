using FluentValidation;
using Tablewise.Application.DTOs.Table;

namespace Tablewise.Application.Validators.Table;

/// <summary>
/// ReorderTablesDto için FluentValidation kuralları.
/// </summary>
public sealed class ReorderTablesDtoValidator : AbstractValidator<ReorderTablesDto>
{
    public ReorderTablesDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sıralama listesi boş olamaz.")
            .Must(x => x.Count <= 100).WithMessage("Tek seferde maksimum 100 masa sıralanabilir.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Masa ID zorunludur.");

            item.RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sıralama değeri 0 veya daha büyük olmalıdır.");
        });
    }
}
