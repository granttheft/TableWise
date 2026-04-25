using FluentValidation;
using Tablewise.Application.DTOs.Auth;

namespace Tablewise.Application.Validators.Auth;

/// <summary>
/// ResetPasswordDto için FluentValidation kuralları.
/// </summary>
public sealed class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    /// <summary>
    /// ResetPasswordDtoValidator constructor.
    /// </summary>
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Sıfırlama token'ı zorunludur.")
            .Length(32).WithMessage("Geçersiz sıfırlama token'ı.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre zorunludur.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(128).WithMessage("Şifre en fazla 128 karakter olabilir.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre tekrarı zorunludur.")
            .Equal(x => x.NewPassword).WithMessage("Şifreler eşleşmiyor.");
    }
}
