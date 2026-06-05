using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed class AddPlatformNoteCommandHandler : IRequestHandler<AddPlatformNoteCommand, Unit>
{
    private readonly IApplicationDbContext _db;

    public AddPlatformNoteCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(AddPlatformNoteCommand request, CancellationToken cancellationToken)
    {
        var tenantExists = await _db.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id == request.TenantId && !t.IsDeleted, cancellationToken);

        if (!tenantExists)
            throw new NotFoundException("Tenant", request.TenantId);

        _db.PlatformNotes.Add(new PlatformNote
        {
            TenantId = request.TenantId,
            Content = request.Content,
            CreatedByEmail = request.CreatedByEmail
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
