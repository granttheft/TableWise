using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Auth;

public sealed class PlatformLoginCommandHandler : IRequestHandler<PlatformLoginCommand, PlatformAuthResultDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPlatformJwtTokenService _jwtService;

    public PlatformLoginCommandHandler(IApplicationDbContext db, IPlatformJwtTokenService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    public async Task<PlatformAuthResultDto> Handle(PlatformLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.PlatformUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Geçersiz e-posta veya şifre.");

        if (!user.IsActive)
            throw new UnauthorizedException("Bu hesap devre dışı bırakılmıştır.");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return new PlatformAuthResultDto(token, user.FullName, user.Role);
    }
}
