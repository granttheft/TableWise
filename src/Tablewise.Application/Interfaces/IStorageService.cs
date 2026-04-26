namespace Tablewise.Application.Interfaces;

/// <summary>
/// Storage servisi interface'i (Cloudflare R2).
/// Dosya yükleme, silme ve URL oluşturma işlemleri.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Presigned upload URL oluşturur.
    /// Client bu URL'e doğrudan PUT request ile dosya yükleyebilir.
    /// </summary>
    /// <param name="key">Dosya key'i (path)</param>
    /// <param name="contentType">Dosya MIME tipi</param>
    /// <param name="expiryMinutes">URL son kullanma süresi (dakika)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Presigned upload URL</returns>
    Task<string> GeneratePresignedUploadUrlAsync(
        string key,
        string contentType,
        int expiryMinutes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Public erişilebilir dosya URL'i döner.
    /// </summary>
    /// <param name="key">Dosya key'i</param>
    /// <returns>Public URL</returns>
    string GetPublicUrl(string key);

    /// <summary>
    /// Dosyayı siler.
    /// </summary>
    /// <param name="key">Dosya key'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    Task DeleteFileAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosyanın var olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="key">Dosya key'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Dosya mevcut mu?</returns>
    Task<bool> FileExistsAsync(string key, CancellationToken cancellationToken = default);
}
