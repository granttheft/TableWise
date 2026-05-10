using MediatR;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Features.Rule.Queries;

/// <summary>
/// ID'ye göre kural getirme query'si.
/// </summary>
public sealed record GetRuleByIdQuery : IRequest<RuleDto>
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public required Guid RuleId { get; init; }
}
