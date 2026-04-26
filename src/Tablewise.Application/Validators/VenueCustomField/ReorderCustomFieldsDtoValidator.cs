using FluentValidation;
using Tablewise.Application.DTOs.VenueCustomField;

namespace Tablewise.Application.Validators.VenueCustomField;

/// <summary>
/// ReorderCustomFieldsDto için FluentValidation kuralları.
/// </summary>
public sealed class ReorderCustomFieldsDtoValidator : AbstractValidator<ReorderCustomFieldsDto>
{
    public ReorderCustomFieldsDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sıralama listesi boş olamaz.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Custom field ID zorunludur.");

            item.RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sıralama değeri 0 veya daha büyük olmalıdır.");
        });
    }
}
