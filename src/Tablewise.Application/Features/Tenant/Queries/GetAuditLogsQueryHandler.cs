using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Audit log listesi sorgusu handler'ı.
/// </summary>
public sealed class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedAuditLogsDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    public GetAuditLogsQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<PagedAuditLogsDto> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner erişebilir
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar audit log'lara erişebilir.");
        }

        // Base query
        var query = _dbContext.AuditLogs
            .Where(a => a.TenantId == tenantId && !a.IsDeleted);

        // Filtreler
        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= request.ToDate.Value);
        }

        // Toplam kayıt sayısı
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        // Sayfalama ve sıralama (en yeni en üstte)
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Action = a.Action,
                PerformedBy = a.PerformedBy,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedAuditLogsDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
