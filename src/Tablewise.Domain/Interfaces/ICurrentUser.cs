using Tablewise.Domain.Enums;

namespace Tablewise.Domain.Interfaces;

/// <summary>
/// Aktif kullanıcı bilgilerini sağlar. JWT token'dan parse edilir.
/// Authorization ve audit log için kullanılır.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Aktif kullanıcının tenant ID'si. Nullable (SuperAdmin için null olabilir).
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Aktif kullanıcının ID'si. Authentication gerekiyorsa null olmamalı.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Aktif kullanıcının rolü. Authorization için kullanılır.
    /// </summary>
    UserRole? Role { get; }

    /// <summary>
    /// Aktif kullanıcının email adresi.
    /// </summary>
    string? Email { get; }
}
