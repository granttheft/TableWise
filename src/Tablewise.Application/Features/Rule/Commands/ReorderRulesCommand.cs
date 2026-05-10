using MediatR;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural sıralama güncelleme komutu.
/// </summary>
public sealed record ReorderRulesCommand : IRequest<Unit>
{
    /// <summary>
    /// Sıralama bilgileri.
    /// </summary>
    public required ReorderRulesDto Dto { get; init; }
}
