using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Platform;

public record PlatformLoginDto(string Email, string Password);

public record PlatformAuthResultDto(string AccessToken, string FullName, PlatformRole Role);
