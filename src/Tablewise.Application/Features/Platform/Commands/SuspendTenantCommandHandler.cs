using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public SuspendTenantCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        tenant.IsActive = !request.Suspend;
        if (request.Suspend && tenant.PlanStatus == PlanStatus.Active)
            tenant.PlanStatus = PlanStatus.Suspended;
        else if (!request.Suspend && tenant.PlanStatus == PlanStatus.Suspended)
            tenant.PlanStatus = PlanStatus.Active;

        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
