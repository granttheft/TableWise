using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public record InvitePlatformUserCommand(InvitePlatformUserDto Dto) : IRequest<PlatformUserDto>;

public sealed class InvitePlatformUserCommandHandler : IRequestHandler<InvitePlatformUserCommand, PlatformUserDto>
{
    private readonly IApplicationDbContext _db;

    public InvitePlatformUserCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlatformUserDto> Handle(InvitePlatformUserCommand request, CancellationToken ct)
    {
        var email = request.Dto.Email.Trim().ToLowerInvariant();

        var exists = await _db.PlatformUsers
            .AnyAsync(u => u.Email == email && !u.IsDeleted, ct);

        if (exists)
            throw new ConflictException($"'{email}' e-posta adresi zaten kayıtlı.");

        var user = new PlatformUser
        {
            Email = email,
            FullName = request.Dto.FullName.Trim(),
            Role = request.Dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        _db.PlatformUsers.Add(user);
        await _db.SaveChangesAsync(ct);

        return new PlatformUserDto(user.Id, user.Email, user.FullName, user.Role, user.IsActive, user.LastLoginAt, user.CreatedAt);
    }
}
