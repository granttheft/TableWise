using FluentValidation;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Validators.Tenant;

/// <summary>
/// GenerateLogoUploadUrlDto için FluentValidation kuralları.
/// </summary>
public sealed class GenerateLogoUploadUrlDtoValidator : AbstractValidator<GenerateLogoUploadUrlDto>
{
    private static readonly string[] AllowedContentTypes = 
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public GenerateLogoUploadUrlDtoValidator()
    {
        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type zorunludur.")
            .Must(BeValidContentType).WithMessage($"Geçerli content type'lar: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("Dosya boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"Dosya boyutu en fazla {MaxFileSizeBytes / 1024 / 1024} MB olabilir.");
    }

    private bool BeValidContentType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType?.ToLowerInvariant());
    }
}
