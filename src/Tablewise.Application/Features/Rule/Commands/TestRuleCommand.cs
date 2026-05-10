using MediatR;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural test komutu.
/// </summary>
public sealed record TestRuleCommand : IRequest<RuleTestResultDto>
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public required Guid RuleId { get; init; }

    /// <summary>
    /// Test parametreleri.
    /// </summary>
    public required TestRuleRequestDto Dto { get; init; }
}
