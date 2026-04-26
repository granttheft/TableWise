using MediatR;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.VenueCustomField.Commands;

/// <summary>
/// Custom field oluşturma komutu.
/// </summary>
public sealed record CreateVenueCustomFieldCommand : IRequest<Guid>
{
    /// <summary>
    /// Venue ID.
    /// </summary>
    public required Guid VenueId { get; init; }

    /// <summary>
    /// Alan etiketi.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Alan tipi.
    /// </summary>
    public required CustomFieldType FieldType { get; init; }

    /// <summary>
    /// Zorunlu mu?
    /// </summary>
    public required bool IsRequired { get; init; }

    /// <summary>
    /// Seçenekler (Select tipi için).
    /// </summary>
    public string? Options { get; init; }
}
