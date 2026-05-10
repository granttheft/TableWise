using MediatR;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural aktif/pasif değiştirme komutu.
/// </summary>
public sealed record ToggleRuleCommand : IRequest<Unit>
{
    /// <summary>
    /// Kural ID.
    /// </summary>
    public required Guid RuleId { get; init; }
}
