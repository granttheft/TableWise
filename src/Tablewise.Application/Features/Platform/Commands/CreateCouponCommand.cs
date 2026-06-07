using MediatR;
using Tablewise.Application.DTOs.Platform;

namespace Tablewise.Application.Features.Platform.Commands;

public record CreateCouponCommand(CreateCouponDto Dto, string CreatedByEmail) : IRequest<CouponDto>;
