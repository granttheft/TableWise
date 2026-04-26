using MediatR;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Tenant bilgilerini güncelleme komutu.
/// Sadece Owner rolü kullanabilir.
/// </summary>
public sealed record UpdateTenantCommand : IRequest<Unit>
{
    /// <summary>
    /// İşletme adı.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Özel ayarlar (JSON formatında).
    /// </summary>
    public string? Settings { get; init; }
}
