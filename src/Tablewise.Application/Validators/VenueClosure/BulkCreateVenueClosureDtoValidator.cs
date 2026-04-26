using FluentValidation;
using Tablewise.Application.DTOs.VenueClosure;

namespace Tablewise.Application.Validators.VenueClosure;

/// <summary>
/// BulkCreateVenueClosureDto için FluentValidation kuralları.
/// </summary>
public sealed class BulkCreateVenueClosureDtoValidator : AbstractValidator<BulkCreateVenueClosureDto>
{
    public BulkCreateVenueClosureDtoValidator()
    {
        RuleFor(x => x.Closures)
            .NotEmpty().WithMessage("En az bir kapalılık kaydı gereklidir.")
            .Must(x => x.Count <= 50).WithMessage("Toplu işlemde maksimum 50 adet kapalılık oluşturabilirsiniz.");

        RuleForEach(x => x.Closures)
            .SetValidator(new CreateVenueClosureDtoValidator());
    }
}
