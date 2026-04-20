using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Moq;
using Tablewise.Infrastructure.Storage;

namespace Tablewise.UnitTests.Infrastructure;

/// <summary>
/// <see cref="R2FileStorageService"/> birim testleri (IAmazonS3 mock).
/// </summary>
public sealed class R2FileStorageServiceTests
{
    private static R2StorageOptions CreateValidOptions() => new()
    {
        AccountId = "test-account",
        AccessKey = "test-access",
        SecretKey = "test-secret",
        BucketName = "tablewise-files"
    };

    /// <summary>
    /// Ön imzalı yükleme URL üretiminde S3 istemcisine doğru parametreler ile istek gider.
    /// </summary>
    [Fact]
    public async Task GeneratePresignedUploadUrlAsync_SendsExpectedParametersToS3()
    {
        var captured = (GetPreSignedUrlRequest?)null;
        var mock = new Mock<IAmazonS3>(MockBehavior.Strict);
        mock.Setup(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(r => captured = r)
            .Returns("https://example.test/presigned-put");

        var options = Options.Create(CreateValidOptions());
        var sut = new R2FileStorageService(mock.Object, options);

        var key = "tenants/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/logos/logo.png";
        var url = await sut.GeneratePresignedUploadUrlAsync(key, "image/png", TimeSpan.FromMinutes(15));

        Assert.Equal("https://example.test/presigned-put", url);
        Assert.NotNull(captured);
        Assert.Equal(options.Value.BucketName, captured!.BucketName);
        Assert.Equal(key, captured.Key);
        Assert.Equal(HttpVerb.PUT, captured.Verb);
        Assert.Equal("image/png", captured.ContentType);
        Assert.True(captured.Expires > DateTime.UtcNow);

        mock.Verify(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()), Times.Once);
    }

    /// <summary>
    /// İzin verilmeyen içerik türünde ön imza üretilmez ve S3 çağrılmaz.
    /// </summary>
    [Fact]
    public void GeneratePresignedUploadUrlAsync_RejectsInvalidContentType()
    {
        var mock = new Mock<IAmazonS3>(MockBehavior.Strict);
        var sut = new R2FileStorageService(mock.Object, Options.Create(CreateValidOptions()));

        Assert.Throws<ArgumentException>(() =>
            _ = sut.GeneratePresignedUploadUrlAsync(
                "tenants/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/logos/x.bin",
                "application/octet-stream",
                TimeSpan.FromMinutes(5)).GetAwaiter().GetResult());

        mock.Verify(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()), Times.Never);
    }

    /// <summary>
    /// Kiracı nesne anahtarı beklenen segments yapısını üretir.
    /// </summary>
    [Theory]
    [InlineData("logos", "brand.webp", "tenants/11111111-1111-1111-1111-111111111111/logos/brand.webp")]
    [InlineData("documents", "menu.pdf", "tenants/11111111-1111-1111-1111-111111111111/documents/menu.pdf")]
    public void BuildTenantKey_ProducesExpectedPath(string folder, string filename, string expected)
    {
        var mock = new Mock<IAmazonS3>();
        var sut = new R2FileStorageService(mock.Object, Options.Create(CreateValidOptions()));

        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var actual = sut.BuildTenantKey(tenantId, folder, filename);

        Assert.Equal(expected, actual);
    }
}
