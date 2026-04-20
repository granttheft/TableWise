namespace Tablewise.Infrastructure.Storage;

/// <summary>
/// Cloudflare R2 (S3 uyumlu) bağlantı ve yükleme limitleri için yapılandırma.
/// </summary>
public sealed class R2StorageOptions
{
    /// <summary>
    /// appsettings.json bölüm adı.
    /// </summary>
    public const string SectionName = "R2";

    /// <summary>
    /// Cloudflare hesap kimliği (R2 endpoint için).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// R2 S3 API erişim anahtarı.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// R2 S3 API gizli anahtarı.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Kova adı.
    /// </summary>
    public string BucketName { get; set; } = "tablewise-files";

    /// <summary>
    /// Genel CDN veya herkese açık taban URL (sonunda / olmadan).
    /// </summary>
    public string PublicUrlBase { get; set; } = string.Empty;

    /// <summary>
    /// Logo yüklemeleri için maksimum bayt (varsayılan 2 MB).
    /// </summary>
    public long LogoMaxUploadBytes { get; set; } = 2L * 1024 * 1024;

    /// <summary>
    /// Logo dışı yüklemeler için maksimum bayt (varsayılan 10 MB).
    /// </summary>
    public long GeneralMaxUploadBytes { get; set; } = 10L * 1024 * 1024;

    /// <summary>
    /// Logo klasörü segmenti; anahtar bu segmenti içeriyorsa <see cref="LogoMaxUploadBytes"/> uygulanır.
    /// </summary>
    public string LogoFolderSegment { get; set; } = "logos";
}
