using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Customer;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Customer.Queries;

/// <summary>
/// Müşteri listesi query'si
/// </summary>
public sealed class GetCustomersQuery : IRequest<List<CustomerDto>>
{
    public string? SearchTerm { get; set; }
    public string? Tier { get; set; }
    public bool? IsBlacklisted { get; set; }
}

/// <summary>
/// GetCustomersQuery handler
/// </summary>
public sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetCustomersQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<List<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var query = _context.Customers
            .Where(c => c.TenantId == tenantId)
            .AsNoTracking();

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(searchLower) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                (c.Phone != null && c.Phone.Contains(request.SearchTerm)));
        }

        // Tier filtresi
        if (!string.IsNullOrWhiteSpace(request.Tier) && Enum.TryParse<CustomerTier>(request.Tier, out var tierEnum))
        {
            query = query.Where(c => c.Tier == tierEnum);
        }

        // Blacklist filtresi
        if (request.IsBlacklisted.HasValue)
        {
            query = query.Where(c => c.IsBlacklisted == request.IsBlacklisted.Value);
        }

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Tier = c.Tier.ToString(),
                TotalVisits = c.TotalVisits,
                LastReservationDate = c.LastReservationAt,
                IsBlacklisted = c.IsBlacklisted,
                BlacklistReason = c.BlacklistReason,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}
