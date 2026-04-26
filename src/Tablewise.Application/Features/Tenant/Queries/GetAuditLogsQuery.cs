using MediatR;
using Tablewise.Application.DTOs.Tenant;

namespace Tablewise.Application.Features.Tenant.Queries;

/// <summary>
/// Audit log listesi sorgusu.
/// Sadece Owner rolü erişebilir.
/// </summary>
public sealed record GetAuditLogsQuery : IRequest<PagedAuditLogsDto>
{
    /// <summary>
    /// Sayfa numarası (1'den başlar).
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Sayfa boyutu.
    /// </summary>
    public int PageSize { get; init; } = 50;

    /// <summary>
    /// Filtreleme - Action türü (opsiyonel).
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// Filtreleme - Entity tipi (opsiyonel).
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// Filtreleme - Başlangıç tarihi (UTC).
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Filtreleme - Bitiş tarihi (UTC).
    /// </summary>
    public DateTime? ToDate { get; init; }
}
