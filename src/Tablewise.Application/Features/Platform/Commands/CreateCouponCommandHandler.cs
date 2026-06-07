using MediatR;
using Microsoft.EntityFrameworkCore;
using Tablewise.Application.DTOs.Platform;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Application.Features.Platform.Commands;

public sealed class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponDto>
{
    private readonly IApplicationDbContext _db;

    public CreateCouponCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var code = request.Dto.Code.ToUpperInvariant().Trim();

        var exists = await _db.Coupons
            .AnyAsync(c => c.Code == code && !c.IsDeleted, cancellationToken);

        if (exists)
            throw new ConflictException($"'{code}' kodlu kupon zaten mevcut.");

        var coupon = new Coupon
        {
            Code = code,
            DiscountType = request.Dto.DiscountType,
            DiscountValue = request.Dto.DiscountValue,
            UsageLimit = request.Dto.UsageLimit,
            ExpiresAt = request.Dto.ExpiresAt,
            ApplicablePlans = request.Dto.ApplicablePlans,
            CreatedByEmail = request.CreatedByEmail,
            IsActive = true,
            UsedCount = 0,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(coupon);
    }

    internal static CouponDto Map(Coupon c) => new(
        c.Id, c.Code, c.DiscountType, c.DiscountValue,
        c.UsageLimit, c.UsedCount, c.ExpiresAt, c.IsActive,
        c.ApplicablePlans, c.CreatedByEmail, c.CreatedAt);
}
