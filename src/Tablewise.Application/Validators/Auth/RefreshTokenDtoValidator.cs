using FluentValidation;
using Tablewise.Application.DTOs.Auth;

namespace Tablewise.Application.Validators.Auth;

/// <summary>
/// RefreshTokenDto için FluentValidation kuralları.
/// </summary>
public sealed class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    /// <summary>
    /// RefreshTokenDtoValidator constructor.
    /// </summary>
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token zorunludur.");
    }
}
