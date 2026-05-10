using MediatR;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.Rule.Queries;

/// <summary>
/// Kural listesi getirme query'si.
/// </summary>
public sealed record GetRulesQuery : IRequest<List<RuleDto>>
{
    /// <summary>
    /// Mekan ID filtresi (nullable).
    /// </summary>
    public Guid? VenueId { get; init; }

    /// <summary>
    /// Aktif durum filtresi (nullable).
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Tetikleyici tip filtresi (nullable).
    /// </summary>
    public RuleTrigger? TriggerType { get; init; }
}
