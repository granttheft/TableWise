using MediatR;
using Tablewise.Application.DTOs.Auth;

namespace Tablewise.Application.Features.Staff.Commands;

/// <summary>
/// Davet kabul komutu.
/// Public endpoint - authentication gerektirmez.
/// </summary>
public sealed record AcceptInvitationCommand : IRequest<AuthResultDto>
{
    /// <summary>
    /// Davet token'ı.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Ad.
    /// </summary>
    public required string FirstName { get; init; }

    /// <summary>
    /// Soyad.
    /// </summary>
    public required string LastName { get; init; }

    /// <summary>
    /// Şifre.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Telefon numarası (opsiyonel).
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// IP adresi (audit için).
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// User agent (audit için).
    /// </summary>
    public string? UserAgent { get; init; }
}
