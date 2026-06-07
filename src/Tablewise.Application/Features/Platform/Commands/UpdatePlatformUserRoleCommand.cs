using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public record UpdatePlatformUserRoleCommand(Guid UserId, PlatformRole NewRole) : IRequest<PlatformUserDto>;

public sealed class UpdatePlatformUserRoleCommandHandler : IRequestHandler<UpdatePlatformUserRoleCommand, PlatformUserDto>
{
    private readonly IApplicationDbContext _db;

    public UpdatePlatformUserRoleCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlatformUserDto> Handle(UpdatePlatformUserRoleCommand request, CancellationToken ct)
    {
        var user = await _db.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, ct)
            ?? throw new NotFoundException("PlatformUser", request.UserId);

        user.Role = request.NewRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new PlatformUserDto(user.Id, user.Email, user.FullName, user.Role, user.IsActive, user.LastLoginAt, user.CreatedAt);
    }
}
