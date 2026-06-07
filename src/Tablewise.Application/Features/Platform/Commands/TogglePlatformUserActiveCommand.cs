using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public record TogglePlatformUserActiveCommand(Guid UserId, Guid RequestingUserId) : IRequest<PlatformUserDto>;

public sealed class TogglePlatformUserActiveCommandHandler : IRequestHandler<TogglePlatformUserActiveCommand, PlatformUserDto>
{
    private readonly IApplicationDbContext _db;

    public TogglePlatformUserActiveCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<PlatformUserDto> Handle(TogglePlatformUserActiveCommand request, CancellationToken ct)
    {
        if (request.UserId == request.RequestingUserId)
            throw new ValidationException("userId", "Kendi hesabınızı deaktif edemezsiniz.");

        var user = await _db.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, ct)
            ?? throw new NotFoundException("PlatformUser", request.UserId);

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new PlatformUserDto(user.Id, user.Email, user.FullName, user.Role, user.IsActive, user.LastLoginAt, user.CreatedAt);
    }
}
