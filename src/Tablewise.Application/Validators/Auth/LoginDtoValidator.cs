using FluentValidation;
using Tablewise.Application.DTOs.Auth;

namespace Tablewise.Application.Validators.Auth;

/// <summary>
/// LoginDto için FluentValidation kuralları.
/// </summary>
public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
{
    /// <summary>
    /// LoginDtoValidator constructor.
    /// </summary>
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.");
    }
}
