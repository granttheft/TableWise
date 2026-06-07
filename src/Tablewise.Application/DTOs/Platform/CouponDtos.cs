using Tablewise.Domain.Enums;

namespace Tablewise.Application.DTOs.Platform;

public record CouponDto(
    Guid Id,
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    int? UsageLimit,
    int UsedCount,
    DateTime? ExpiresAt,
    bool IsActive,
    string? ApplicablePlans,
    string CreatedByEmail,
    DateTime CreatedAt);

public record CreateCouponDto(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    int? UsageLimit,
    DateTime? ExpiresAt,
    string? ApplicablePlans);

public record PlanPricingDto(
    Guid Id,
    string Name,
    string Tier,
    decimal MonthlyPriceTry,
    decimal YearlyPriceTry,
    bool IsVisible);

public record UpdatePlanPricingDto(
    decimal MonthlyPriceTry,
    decimal YearlyPriceTry);
