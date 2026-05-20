using MediatR;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Taslak kural test komutu (kaydetmeden değerlendirme).
/// </summary>
public sealed record TestDraftRuleCommand : IRequest<RuleTestResultDto>
{
    /// <summary>
    /// Test isteği gövdesi.
    /// </summary>
    public required TestDraftRuleRequestDto Dto { get; init; }
}
