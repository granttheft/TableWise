using MediatR;

namespace Tablewise.Application.Features.Platform.Commands;

public record DeactivateCouponCommand(Guid CouponId) : IRequest;
