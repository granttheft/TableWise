using MediatR;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Features.Rule.Queries;

/// <summary>
/// Kural istatistikleri getirme query'si.
/// </summary>
public sealed record GetRuleStatsQuery : IRequest<List<RuleStatDto>>
{
    /// <summary>
    /// Mekan ID filtresi (nullable).
    /// </summary>
    public Guid? VenueId { get; init; }
}
