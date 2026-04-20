namespace Tablewise.Application.Interfaces;

/// <summary>
/// Cloudflare R2 (S3 uyumlu) üzerinde nesne depolama ve ön imzalı URL üretimi için sözleşme.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Belirtilen anahtar için HTTP PUT ile yükleme yapılacak ön imzalı URL üretir.
    /// </summary>
    /// <param name="key">Depo nesne anahtarı (ör. <see cref="BuildTenantKey"/> çıktısı).</param>
    /// <param name="contentType">İçerik türü (yalnızca izin verilen görüntü türleri).</param>
    /// <param name="expiry">URL geçerlilik süresi.</param>
    /// <returns>Ön imzalı yükleme URL'si.</returns>
    Task<string> GeneratePresignedUploadUrlAsync(string key, string contentType, TimeSpan expiry);

    /// <summary>
    /// Belirtilen anahtar için HTTP GET ile indirilebilecek ön imzalı URL üretir.
    /// </summary>
    /// <param name="key">Depo nesne anahtarı.</param>
    /// <param name="expiry">URL geçerlilik süresi.</param>
    /// <returns>Ön imzalı indirme URL'si.</returns>
    Task<string> GetPresignedDownloadUrlAsync(string key, TimeSpan expiry);

    /// <summary>
    /// Nesneyi depodan siler (idempotent: yoksa hata fırlatılmaz).
    /// </summary>
    /// <param name="key">Depo nesne anahtarı.</param>
    Task DeleteAsync(string key);

    /// <summary>
    /// Nesnenin depoda var olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="key">Depo nesne anahtarı.</param>
    /// <returns>Var ise true.</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Kiracıya ait, klasör ve dosya adı ile güvenli nesne anahtarı üretir.
    /// Örnek: <c>tenants/{tenantId}/logos/logo.webp</c>
    /// </summary>
    /// <param name="tenantId">Tenant kimliği.</param>
    /// <param name="folder">Mantıksal klasör (ör. logos, documents).</param>
    /// <param name="filename">Dosya adı (yol bileşeni içeremez).</param>
    /// <returns>S3/R2 nesne anahtarı.</returns>
    string BuildTenantKey(Guid tenantId, string folder, string filename);
}
