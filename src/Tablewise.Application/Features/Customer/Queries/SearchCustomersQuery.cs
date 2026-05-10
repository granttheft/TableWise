using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Customer;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Customer.Queries;

/// <summary>
/// Müşteri arama query'si (typeahead için)
/// </summary>
public sealed class SearchCustomersQuery : IRequest<List<CustomerDto>>
{
    public string SearchTerm { get; set; } = string.Empty;
}

/// <summary>
/// SearchCustomersQuery handler
/// </summary>
public sealed class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public SearchCustomersQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<List<CustomerDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 2)
        {
            return new List<CustomerDto>();
        }

        var tenantId = _tenantService.GetCurrentTenantId();
        var searchLower = request.SearchTerm.ToLower();

        var customers = await _context.Customers
            .Where(c => c.TenantId == tenantId)
            .Where(c =>
                c.FullName.ToLower().Contains(searchLower) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                (c.Phone != null && c.Phone.Contains(request.SearchTerm)))
            .Where(c => !c.IsBlacklisted) // Blacklist'teki müşterileri hariç tut
            .OrderBy(c => c.FullName)
            .Take(10) // Max 10 sonuç
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
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return customers;
    }
}
