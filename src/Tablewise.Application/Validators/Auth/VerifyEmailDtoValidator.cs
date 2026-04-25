using FluentValidation;
using Tablewise.Application.DTOs.Auth;

namespace Tablewise.Application.Validators.Auth;

/// <summary>
/// VerifyEmailDto için FluentValidation kuralları.
/// </summary>
public sealed class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
{
    /// <summary>
    /// VerifyEmailDtoValidator constructor.
    /// </summary>
    public VerifyEmailDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Doğrulama token'ı zorunludur.")
            .Length(32).WithMessage("Geçersiz doğrulama token'ı.");
    }
}
