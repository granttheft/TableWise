using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Staff;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Staff.Queries;

/// <summary>
/// Personel listesi sorgusu handler'ı.
/// </summary>
public sealed class ListStaffQueryHandler : IRequestHandler<ListStaffQuery, List<StaffMemberDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// ListStaffQueryHandler constructor.
    /// </summary>
    public ListStaffQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<List<StaffMemberDto>> Handle(ListStaffQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar personel listesini görebilir.");
        }

        var query = _dbContext.Users
            .Where(u => u.TenantId == tenantId && !u.IsDeleted);

        if (request.ActiveOnly)
        {
            query = query.Where(u => u.IsActive);
        }

        var staff = await query
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FirstName)
            .Select(u => new StaffMemberDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                InvitedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return staff;
    }
}
