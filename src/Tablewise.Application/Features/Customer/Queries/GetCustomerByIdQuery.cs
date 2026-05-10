using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Customer;
using Tablewise.Application.Exceptions;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Customer.Queries;

/// <summary>
/// Müşteri detay query'si
/// </summary>
public sealed class GetCustomerByIdQuery : IRequest<CustomerDto>
{
    public Guid CustomerId { get; set; }
}

/// <summary>
/// GetCustomerByIdQuery handler
/// </summary>
public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public GetCustomerByIdQueryHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var customer = await _context.Customers
            .Where(c => c.Id == request.CustomerId && c.TenantId == tenantId)
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
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Customer), request.CustomerId);
        }

        return customer;
    }
}
