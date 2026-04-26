namespace Tablewise.Application.DTOs.VenueClosure;

/// <summary>
/// Toplu kapalılık oluşturma DTO'su.
/// </summary>
public sealed record BulkCreateVenueClosureDto
{
    /// <summary>
    /// Kapalılık listesi (maksimum 50 adet).
    /// </summary>
    public required List<CreateVenueClosureDto> Closures { get; init; }
}
