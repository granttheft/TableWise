using MediatR;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural oluşturma komutu.
/// </summary>
public sealed record CreateRuleCommand : IRequest<Guid>
{
    /// <summary>
    /// Kural bilgileri.
    /// </summary>
    public required CreateRuleDto Dto { get; init; }
}
