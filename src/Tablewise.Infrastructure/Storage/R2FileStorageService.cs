using System.Globalization;
using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Tablewise.Application.Interfaces;

namespace Tablewise.Infrastructure.Storage;

/// <summary>
/// Cloudflare R2 üzerinde S3 uyumlu API ile dosya depolama (ön imzalı URL, silme, varlık kontrolü).
/// </summary>
/// <remarks>
/// Yükleme boyutu üst sınırı ön imzalı URL ile sunucuda zorunlu kılınamaz; <see cref="R2StorageOptions.LogoMaxUploadBytes"/>
/// ve <see cref="R2StorageOptions.GeneralMaxUploadBytes"/> değerleri uygulama katmanında doğrulanmalıdır.
/// Logo yüklemeleri için anahtarda <c>/{LogoFolderSegment}/</c> segmenti kullanılması önerilir.
/// </remarks>
public sealed class R2FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly R2StorageOptions _options;

    /// <summary>
    /// R2FileStorageService constructor.
    /// </summary>
    /// <param name="s3">R2 endpoint'ine yönlendirilmiş S3 istemcisi.</param>
    /// <param name="options">R2 yapılandırması.</param>
    public R2FileStorageService(IAmazonS3 s3, IOptions<R2StorageOptions> options)
    {
        _s3 = s3 ?? throw new ArgumentNullException(nameof(s3));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Yalnızca jpeg/png/webp içerik türlerine izin verilir. Boyut limitleri yapılandırmada tutulur; istemci ve API doğrulaması önerilir.
    /// </remarks>
    public Task<string> GeneratePresignedUploadUrlAsync(string key, string contentType, TimeSpan expiry)
    {
        EnsureConfigured();
        ValidateKey(key);
        ValidateContentType(contentType);
        ValidateExpiry(expiry);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        var url = _s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public Task<string> GetPresignedDownloadUrlAsync(string key, TimeSpan expiry)
    {
        EnsureConfigured();
        ValidateKey(key);
        ValidateExpiry(expiry);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        var url = _s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string key)
    {
        EnsureConfigured();
        ValidateKey(key);

        await _s3.DeleteObjectAsync(_options.BucketName, key).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key)
    {
        EnsureConfigured();
        ValidateKey(key);

        try
        {
            await _s3.GetObjectMetadataAsync(_options.BucketName, key).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public string BuildTenantKey(Guid tenantId, string folder, string filename)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId boş olamaz.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new ArgumentException("Klasör boş olamaz.", nameof(folder));
        }

        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException("Dosya adı boş olamaz.", nameof(filename));
        }

        var normalizedFolder = NormalizeFolder(folder);
        var safeFileName = SanitizeFileName(filename);

        return string.Format(CultureInfo.InvariantCulture, "{0}{1:D}/{2}/{3}", FileStorageConstants.TenantsKeyPrefix, tenantId, normalizedFolder, safeFileName);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.AccountId)
            || string.IsNullOrWhiteSpace(_options.AccessKey)
            || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException(
                "R2 yapılandırması eksik. 'R2:AccountId', 'R2:AccessKey' ve 'R2:SecretKey' değerlerini user-secrets veya ortam değişkenleri ile sağlayın.");
        }

        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("R2 yapılandırması eksik: 'R2:BucketName' boş olamaz.");
        }
    }

    private static void ValidateExpiry(TimeSpan expiry)
    {
        if (expiry <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiry), "Geçerlilik süresi sıfırdan büyük olmalıdır.");
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Anahtar boş olamaz.", nameof(key));
        }

        if (key.Contains("..", StringComparison.Ordinal) || key.StartsWith('/'))
        {
            throw new ArgumentException("Geçersiz nesne anahtarı.", nameof(key));
        }
    }

    private static void ValidateContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("İçerik türü boş olamaz.", nameof(contentType));
        }

        if (!FileStorageConstants.AllowedImageContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "İzin verilmeyen içerik türü. Yalnızca image/jpeg, image/png ve image/webp desteklenir.",
                nameof(contentType));
        }
    }

    private static string NormalizeFolder(string folder)
    {
        var trimmed = folder.Trim().Replace('\\', '/').Trim('/');
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Klasör boş olamaz.", nameof(folder));
        }

        if (trimmed.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("Klasör yolu geçersiz.", nameof(folder));
        }

        return trimmed;
    }

    private static string SanitizeFileName(string filename)
    {
        var name = Path.GetFileName(filename.Replace('\\', '/').Trim());
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dosya adı geçersiz.", nameof(filename));
        }

        if (name.Contains("..", StringComparison.Ordinal) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Dosya adı geçersiz karakterler içeriyor.", nameof(filename));
        }

        return name;
    }
}
