using MediatR;
using Microsoft.Extensions.Logging;
using Tablewise.Application.DTOs.Tenant;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Logo upload için presigned URL oluşturma komutu handler'ı.
/// </summary>
public sealed class GenerateLogoUploadUrlCommandHandler : IRequestHandler<GenerateLogoUploadUrlCommand, LogoUploadUrlDto>
{
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IStorageService _storageService;
    private readonly ILogger<GenerateLogoUploadUrlCommandHandler> _logger;

    private static readonly string[] AllowedContentTypes = 
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public GenerateLogoUploadUrlCommandHandler(
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IStorageService storageService,
        ILogger<GenerateLogoUploadUrlCommandHandler> logger)
    {
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<LogoUploadUrlDto> Handle(GenerateLogoUploadUrlCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar logo yükleyebilir.");
        }

        // Content type kontrolü
        if (!AllowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
        {
            throw new BusinessRuleException(
                $"Desteklenmeyen dosya tipi. İzin verilen tipler: {string.Join(", ", AllowedContentTypes)}",
                "INVALID_CONTENT_TYPE");
        }

        // Dosya boyutu kontrolü
        if (request.FileSizeBytes > MaxFileSizeBytes)
        {
            throw new BusinessRuleException(
                $"Dosya boyutu çok büyük. Maksimum: {MaxFileSizeBytes / 1024 / 1024} MB",
                "FILE_TOO_LARGE");
        }

        if (request.FileSizeBytes <= 0)
        {
            throw new BusinessRuleException("Geçersiz dosya boyutu.", "INVALID_FILE_SIZE");
        }

        // Benzersiz dosya key'i oluştur
        var fileExtension = GetFileExtension(request.ContentType);
        var fileKey = $"tenants/{tenantId}/logo-{Guid.NewGuid()}{fileExtension}";

        // Presigned URL oluştur (15 dakika geçerli)
        var uploadUrl = await _storageService.GeneratePresignedUploadUrlAsync(
            fileKey,
            request.ContentType,
            expiryMinutes: 15,
            cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Logo upload URL oluşturuldu: TenantId={TenantId}, FileKey={FileKey}",
            tenantId, fileKey);

        return new LogoUploadUrlDto
        {
            UploadUrl = uploadUrl,
            FileKey = fileKey,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            MaxFileSizeBytes = MaxFileSizeBytes,
            AllowedContentTypes = AllowedContentTypes
        };
    }

    private static string GetFileExtension(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}
