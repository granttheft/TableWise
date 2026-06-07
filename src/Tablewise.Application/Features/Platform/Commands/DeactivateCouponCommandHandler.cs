using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed class DeactivateCouponCommandHandler : IRequestHandler<DeactivateCouponCommand>
{
    private readonly IApplicationDbContext _db;

    public DeactivateCouponCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeactivateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.CouponId && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Coupon", request.CouponId);

        coupon.IsActive = false;
        coupon.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
