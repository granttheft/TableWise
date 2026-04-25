using FluentValidation;
using Tablewise.Application.DTOs.Auth;

namespace Tablewise.Application.Validators.Auth;

/// <summary>
/// ForgotPasswordDto için FluentValidation kuralları.
/// </summary>
public sealed class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    /// <summary>
    /// ForgotPasswordDtoValidator constructor.
    /// </summary>
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
    }
}
