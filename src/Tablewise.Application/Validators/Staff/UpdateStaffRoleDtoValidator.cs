using FluentValidation;
using Tablewise.Application.DTOs.Staff;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Validators.Staff;

/// <summary>
/// UpdateStaffRoleDto için FluentValidation kuralları.
/// </summary>
public sealed class UpdateStaffRoleDtoValidator : AbstractValidator<UpdateStaffRoleDto>
{
    /// <summary>
    /// UpdateStaffRoleDtoValidator constructor.
    /// </summary>
    public UpdateStaffRoleDtoValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Geçerli bir rol seçiniz.")
            .NotEqual(UserRole.SuperAdmin).WithMessage("SuperAdmin rolü atanamaz.");
    }
}
