namespace Tablewise.Infrastructure.Storage;

/// <summary>
/// Dosya depolama için sabitler (içerik türleri ve anahtar kuralları).
/// </summary>
internal static class FileStorageConstants
{
    /// <summary>
    /// Ön imzalı yükleme için izin verilen görüntü içerik türleri.
    /// </summary>
    internal static readonly string[] AllowedImageContentTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    /// <summary>
    /// Kiracı anahtarları için zorunlu önek.
    /// </summary>
    internal const string TenantsKeyPrefix = "tenants/";
}
