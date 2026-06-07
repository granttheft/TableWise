using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Platform;

public record PlatformUserDto(
    Guid Id,
    string Email,
    string FullName,
    PlatformRole Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public record InvitePlatformUserDto(
    string Email,
    string FullName,
    PlatformRole Role,
    string Password);

public record UpdatePlatformUserRoleDto(PlatformRole NewRole);
