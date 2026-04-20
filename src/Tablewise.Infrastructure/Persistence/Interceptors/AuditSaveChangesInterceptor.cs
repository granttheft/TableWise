using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Audit log interceptor. Değişiklikleri AuditLog tablosuna kaydeder.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// AuditSaveChangesInterceptor constructor.
    /// </summary>
    /// <param name="currentUser">Current user</param>
    public AuditSaveChangesInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// SaveChangesAsync sonrası çağrılır. Audit log'ları kaydeder.
    /// </summary>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await CreateAuditLogs(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Değişiklikleri AuditLog tablosuna kaydeder.
    /// </summary>
    private async Task CreateAuditLogs(DbContext context, CancellationToken cancellationToken)
    {
        var auditLogs = new List<AuditLog>();
        var performedBy = _currentUser.Email ?? "Sistem";
        var userId = _currentUser.UserId;
        var tenantId = _currentUser.TenantId ?? Guid.Empty;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // AuditLog entity'sini loglama (recursion önleme)
            if (entry.Entity is AuditLog)
                continue;

            // Sadece Added, Modified, Deleted state'leri logla
            if (entry.State != EntityState.Added &&
                entry.State != EntityState.Modified &&
                entry.State != EntityState.Deleted)
                continue;

            var entityType = entry.Entity.GetType().Name;
            var entityId = GetEntityId(entry);
            var action = GetActionName(entry.State);

            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = userId,
                PerformedBy = performedBy,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = GetOldValue(entry),
                NewValue = GetNewValue(entry),
                CreatedAt = DateTime.UtcNow
            };

            auditLogs.Add(auditLog);
        }

        if (auditLogs.Any())
        {
            await context.Set<AuditLog>().AddRangeAsync(auditLogs, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Entity ID'sini alır.
    /// </summary>
    private static string? GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProperty?.CurrentValue?.ToString();
    }

    /// <summary>
    /// Action adını EntityState'e göre döndürür.
    /// </summary>
    private static string GetActionName(EntityState state)
    {
        return state switch
        {
            EntityState.Added => "CREATED",
            EntityState.Modified => "UPDATED",
            EntityState.Deleted => "DELETED",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// Eski değeri JSON olarak serialize eder.
    /// </summary>
    private static string? GetOldValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
            return null;

        var oldValues = entry.Properties
            .Where(p => p.IsModified && !IsSensitiveProperty(p.Metadata.Name))
            .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);

        return oldValues.Any()
            ? System.Text.Json.JsonSerializer.Serialize(oldValues)
            : null;
    }

    /// <summary>
    /// Yeni değeri JSON olarak serialize eder.
    /// </summary>
    private static string? GetNewValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.State == EntityState.Deleted)
            return null;

        var newValues = entry.Properties
            .Where(p => (entry.State == EntityState.Added || p.IsModified) && !IsSensitiveProperty(p.Metadata.Name))
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);

        return newValues.Any()
            ? System.Text.Json.JsonSerializer.Serialize(newValues)
            : null;
    }

    /// <summary>
    /// KVKK uyumu: Hassas property'leri loglamayı engeller.
    /// </summary>
    private static bool IsSensitiveProperty(string propertyName)
    {
        var sensitiveProperties = new[]
        {
            "PasswordHash",
            "PasswordResetToken",
            "EmailVerificationToken",
            "IyzicoCustomerId",
            "IyzicoSubscriptionId",
            "DepositPaymentRef"
        };

        return sensitiveProperties.Contains(propertyName);
    }
}
