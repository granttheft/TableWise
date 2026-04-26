namespace Tablewise.Application.DTOs.Tenant;

/// <summary>
/// Audit log DTO'su.
/// </summary>
public sealed record AuditLogDto
{
    /// <summary>
    /// Log ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Yapılan işlem.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// İşlemi yapan.
    /// </summary>
    public required string PerformedBy { get; init; }

    /// <summary>
    /// Entity tipi.
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// Entity ID.
    /// </summary>
    public string? EntityId { get; init; }

    /// <summary>
    /// IP adresi.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Oluşturulma tarihi.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}

/// <summary>
/// Sayfalı audit log sonucu.
/// </summary>
public sealed record PagedAuditLogsDto
{
    /// <summary>
    /// Audit log listesi.
    /// </summary>
    public required List<AuditLogDto> Items { get; init; }

    /// <summary>
    /// Toplam kayıt sayısı.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Sayfa numarası.
    /// </summary>
    public required int PageNumber { get; init; }

    /// <summary>
    /// Sayfa boyutu.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Toplam sayfa sayısı.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Bir sonraki sayfa var mı?
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Bir önceki sayfa var mı?
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
