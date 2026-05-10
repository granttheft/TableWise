namespace Tablewise.Application.Interfaces;

/// <summary>
/// Tenant servisi interface'i.
/// HTTP context'ten mevcut tenant bilgisini almak için kullanılır.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Mevcut HTTP request'teki tenant ID'yi döner.
    /// JWT token'dan veya claim'lerden alınır.
    /// </summary>
    /// <returns>Tenant ID</returns>
    Guid GetCurrentTenantId();

    /// <summary>
    /// Mevcut HTTP request'teki user ID'yi döner.
    /// </summary>
    /// <returns>User ID</returns>
    Guid GetCurrentUserId();

    /// <summary>
    /// Mevcut kullanıcının rolünü döner.
    /// </summary>
    /// <returns>Kullanıcı rolü (Owner, Staff)</returns>
    string GetCurrentUserRole();
}
