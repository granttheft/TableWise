using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Platform.Queries;

public record GetPlatformUsersQuery : IRequest<List<PlatformUserDto>>;

public class GetPlatformUsersQueryHandler : IRequestHandler<GetPlatformUsersQuery, List<PlatformUserDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPlatformUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<PlatformUserDto>> Handle(GetPlatformUsersQuery request, CancellationToken ct)
    {
        return await _db.PlatformUsers
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new PlatformUserDto(
                u.Id,
                u.Email,
                u.FullName,
                u.Role,
                u.IsActive,
                u.LastLoginAt,
                u.CreatedAt))
            .ToListAsync(ct);
    }
}
