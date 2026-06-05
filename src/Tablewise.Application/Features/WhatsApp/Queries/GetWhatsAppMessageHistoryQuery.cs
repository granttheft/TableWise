using MediatR;
using Tablewise.Application.DTOs.Common;
using Tablewise.Application.DTOs.WhatsApp;
using Tablewise.Domain.Enums;

namespace Tablewise.Application.Features.WhatsApp.Queries;

/// <summary>
/// Tenant'ın WhatsApp mesaj geçmişini getirir (sayfalı, filtreli).
/// </summary>
public sealed record GetWhatsAppMessageHistoryQuery : IRequest<PagedResult<WhatsAppMessageHistoryDto>>
{
    /// <summary>
    /// Sayfa numarası (1 tabanlı).
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Sayfa boyutu. Maksimum 100.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Durum filtresi (opsiyonel).
    /// </summary>
    public WhatsAppMessageStatus? Status { get; init; }

    /// <summary>
    /// Başlangıç tarihi filtresi (UTC, opsiyonel).
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// Bitiş tarihi filtresi (UTC, opsiyonel).
    /// </summary>
    public DateTime? To { get; init; }
}
