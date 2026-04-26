using FluentValidation;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Validators.Tenant;

/// <summary>
/// ConfirmLogoUploadDto için FluentValidation kuralları.
/// </summary>
public sealed class ConfirmLogoUploadDtoValidator : AbstractValidator<ConfirmLogoUploadDto>
{
    public ConfirmLogoUploadDtoValidator()
    {
        RuleFor(x => x.FileKey)
            .NotEmpty().WithMessage("FileKey zorunludur.")
            .MaximumLength(500).WithMessage("FileKey en fazla 500 karakter olabilir.")
            .Must(BeValidFileKey).WithMessage("Geçersiz file key formatı.");
    }

    private bool BeValidFileKey(string fileKey)
    {
        // FileKey formatı: tenants/{guid}/logo-{guid}.{ext}
        return fileKey.StartsWith("tenants/") && fileKey.Contains("/logo-");
    }
}
