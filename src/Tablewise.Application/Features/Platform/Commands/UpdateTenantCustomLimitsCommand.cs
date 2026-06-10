using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Application.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed record UpdateTenantCustomLimitsCommand(
    Guid TenantId,
    UpdateTenantCustomLimitsDto Dto) : IRequest;

public sealed class UpdateTenantCustomLimitsCommandHandler
    : IRequestHandler<UpdateTenantCustomLimitsCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateTenantCustomLimitsCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateTenantCustomLimitsCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Tenant", request.TenantId);

        var dto = request.Dto;

        if (dto.MaxVenues is null && dto.MaxTables is null && dto.MaxRules is null
            && dto.MaxReservationsPerMonth is null && dto.MaxStaffAccounts is null)
        {
            tenant.CustomLimitsJson = null;
        }
        else
        {
            var dict = new Dictionary<string, int>();
            if (dto.MaxVenues.HasValue)              dict["maxVenues"]              = dto.MaxVenues.Value;
            if (dto.MaxTables.HasValue)              dict["maxTables"]              = dto.MaxTables.Value;
            if (dto.MaxRules.HasValue)               dict["maxRules"]               = dto.MaxRules.Value;
            if (dto.MaxReservationsPerMonth.HasValue) dict["maxReservationsPerMonth"] = dto.MaxReservationsPerMonth.Value;
            if (dto.MaxStaffAccounts.HasValue)       dict["maxStaffAccounts"]       = dto.MaxStaffAccounts.Value;

            tenant.CustomLimitsJson = JsonSerializer.Serialize(dict);
        }

        tenant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
