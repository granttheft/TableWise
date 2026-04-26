using MediatR;
using Tablewise.Application.DTOs.Staff;

namespace Tablewise.Application.Features.Staff.Queries;

/// <summary>
/// Personel listesi sorgusu.
/// Sadece Owner kullanabilir.
/// </summary>
public sealed record ListStaffQuery : IRequest<List<StaffMemberDto>>
{
    /// <summary>
    /// Sadece aktif kullanıcılar mı? (default: true)
    /// </summary>
    public bool ActiveOnly { get; init; } = true;
}
