using MediatR;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural güncelleme komutu.
/// </summary>
public sealed record UpdateRuleCommand : IRequest<Unit>
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public required Guid RuleId { get; init; }

    /// <summary>
    /// Kural bilgileri.
    /// </summary>
    public required UpdateRuleDto Dto { get; init; }
}
