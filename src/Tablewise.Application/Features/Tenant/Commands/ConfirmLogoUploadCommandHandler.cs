using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Tenant.Commands;

/// <summary>
/// Logo upload onaylama komutu handler'ı.
/// </summary>
public sealed class ConfirmLogoUploadCommandHandler : IRequestHandler<ConfirmLogoUploadCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IStorageService _storageService;
    private readonly ILogger<ConfirmLogoUploadCommandHandler> _logger;

    public ConfirmLogoUploadCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IStorageService storageService,
        ILogger<ConfirmLogoUploadCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ConfirmLogoUploadCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar logo yükleyebilir.");
        }

        // FileKey'in tenant'a ait olduğunu doğrula
        if (!request.FileKey.StartsWith($"tenants/{tenantId}/"))
        {
            throw new ForbiddenException("Geçersiz dosya key'i.");
        }

        // Dosyanın gerçekten upload edildiğini doğrula
        var fileExists = await _storageService.FileExistsAsync(request.FileKey, cancellationToken)
            .ConfigureAwait(false);

        if (!fileExists)
        {
            throw new BusinessRuleException(
                "Dosya bulunamadı. Upload işlemi tamamlanmamış olabilir.",
                "FILE_NOT_FOUND");
        }

        // Tenant'ı bul
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (tenant == null)
        {
            throw new NotFoundException("Tenant", tenantId);
        }

        // Eski logoyu sil (eğer varsa)
        var oldLogoUrl = ExtractLogoUrlFromSettings(tenant.Settings);
        if (!string.IsNullOrEmpty(oldLogoUrl))
        {
            try
            {
                var oldKey = ExtractKeyFromUrl(oldLogoUrl);
                if (!string.IsNullOrEmpty(oldKey))
                {
                    await _storageService.DeleteFileAsync(oldKey, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Eski logo silinemedi: {OldLogoUrl}", oldLogoUrl);
            }
        }

        // Yeni logo URL'ini al
        var newLogoUrl = _storageService.GetPublicUrl(request.FileKey);

        // Settings JSON'ı güncelle
        var settings = UpdateSettingsWithLogo(tenant.Settings, newLogoUrl);
        tenant.Settings = settings;
        tenant.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = _currentUser.UserId,
            PerformedBy = _currentUser.Email ?? "System",
            Action = "LOGO_UPLOADED",
            EntityType = "Tenant",
            EntityId = tenantId.ToString(),
            OldValue = oldLogoUrl,
            NewValue = newLogoUrl,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Logo upload onaylandı: TenantId={TenantId}, FileKey={FileKey}", tenantId, request.FileKey);

        return Unit.Value;
    }

    private static string? ExtractLogoUrlFromSettings(string? settingsJson)
    {
        if (string.IsNullOrEmpty(settingsJson))
            return null;

        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(settingsJson);
            if (doc.RootElement.TryGetProperty("logoUrl", out var logoUrlElement))
            {
                return logoUrlElement.GetString();
            }
        }
        catch
        {
            // JSON parse hatası yutulur
        }

        return null;
    }

    private static string UpdateSettingsWithLogo(string? existingSettings, string logoUrl)
    {
        var settings = new Dictionary<string, object>();

        // Mevcut settings'i parse et
        if (!string.IsNullOrEmpty(existingSettings))
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(existingSettings);
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    settings[property.Name] = property.Value.Clone();
                }
            }
            catch
            {
                // JSON parse hatası, boş settings ile devam
            }
        }

        // logoUrl ekle/güncelle
        settings["logoUrl"] = logoUrl;

        return System.Text.Json.JsonSerializer.Serialize(settings);
    }

    private static string? ExtractKeyFromUrl(string url)
    {
        // URL'den key çıkarma (örnek: https://cdn.tablewise.com/tenants/xxx/logo-yyy.jpg -> tenants/xxx/logo-yyy.jpg)
        var uri = new Uri(url);
        return uri.AbsolutePath.TrimStart('/');
    }
}
