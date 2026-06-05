using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed class UpdateTenantPlanCommandHandler : IRequestHandler<UpdateTenantPlanCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public UpdateTenantPlanCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateTenantPlanCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var planExists = await _db.Plans
            .IgnoreQueryFilters()
            .AnyAsync(p => p.Id == request.PlanId && !p.IsDeleted, cancellationToken);

        if (!planExists)
            throw new NotFoundException("Plan", request.PlanId);

        tenant.PlanId = request.PlanId;
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
