using MediatR;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural test komutu.
/// </summary>
public sealed record TestRuleCommand : IRequest<RuleEvaluationResult>
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
