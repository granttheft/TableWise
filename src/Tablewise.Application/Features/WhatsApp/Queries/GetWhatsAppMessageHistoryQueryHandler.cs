using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Common;
using Tablewise.Application.DTOs.WhatsApp;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.WhatsApp.Queries;

/// <summary>
/// GetWhatsAppMessageHistoryQuery handler'ı.
/// </summary>
public sealed class GetWhatsAppMessageHistoryQueryHandler
    : IRequestHandler<GetWhatsAppMessageHistoryQuery, PagedResult<WhatsAppMessageHistoryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    private const int MaxPageSize = 100;

    /// <summary>
    /// Handler constructor.
    /// </summary>
    public GetWhatsAppMessageHistoryQueryHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<PagedResult<WhatsAppMessageHistoryDto>> Handle(
        GetWhatsAppMessageHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var pageSize = Math.Min(request.PageSize, MaxPageSize);
        var page = Math.Max(request.Page, 1);

        var query = _dbContext.WhatsAppMessages
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && !m.IsDeleted);

        if (request.Status.HasValue)
            query = query.Where(m => m.Status == request.Status.Value);

        if (request.From.HasValue)
            query = query.Where(m => m.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(m => m.CreatedAt <= request.To.Value);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new WhatsAppMessageHistoryDto
            {
                Id = m.Id,
                ToPhone = m.ToPhone,
                Template = m.Template,
                Status = m.Status,
                SentAt = m.SentAt,
                DeliveredAt = m.DeliveredAt,
                ErrorMessage = m.ErrorMessage,
                ReservationId = m.ReservationId
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<WhatsAppMessageHistoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
