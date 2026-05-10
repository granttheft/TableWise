using MediatR;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural silme komutu.
/// </summary>
public sealed record DeleteRuleCommand : IRequest<Unit>
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public required Guid RuleId { get; init; }
}
