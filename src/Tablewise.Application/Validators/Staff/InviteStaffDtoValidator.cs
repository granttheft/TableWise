using FluentValidation;
using Tablewise.Application.DTOs.Staff;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Validators.Staff;

/// <summary>
/// InviteStaffDto için FluentValidation kuralları.
/// </summary>
public sealed class InviteStaffDtoValidator : AbstractValidator<InviteStaffDto>
{
    /// <summary>
    /// InviteStaffDtoValidator constructor.
    /// </summary>
    public InviteStaffDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(256).WithMessage("Email en fazla 256 karakter olabilir.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçerli bir rol seçiniz.")
            .NotEqual(UserRole.SuperAdmin).WithMessage("SuperAdmin rolü atanamaz.");

        RuleFor(x => x.Message)
            .MaximumLength(500).WithMessage("Mesaj en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}
