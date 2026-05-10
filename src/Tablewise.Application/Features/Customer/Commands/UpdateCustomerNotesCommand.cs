using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Customer;
using Tablewise.Application.Exceptions;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Customer.Commands;

/// <summary>
/// Müşteri notları güncelleme command'ı
/// </summary>
public sealed class UpdateCustomerNotesCommand : IRequest<CustomerDto>
{
    public Guid CustomerId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// UpdateCustomerNotesCommand handler
/// </summary>
public sealed class UpdateCustomerNotesCommandHandler : IRequestHandler<UpdateCustomerNotesCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;

    public UpdateCustomerNotesCommandHandler(
        IApplicationDbContext context,
        ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerNotesCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var customer = await _context.Customers
            .Where(c => c.Id == request.CustomerId && c.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Customer), request.CustomerId);
        }

        customer.Notes = request.Notes;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Tier = customer.Tier,
            TotalVisits = customer.TotalVisits,
            LastReservationDate = customer.LastReservationDate,
            IsBlacklisted = customer.IsBlacklisted,
            BlacklistReason = customer.BlacklistReason,
            Notes = customer.Notes,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
