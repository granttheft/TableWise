using MediatR;
using Tablewise.Application.DTOs.Platform;

namespace Tablewise.Application.Features.Platform.Auth;

public sealed record PlatformLoginCommand(string Email, string Password) : IRequest<PlatformAuthResultDto>;
