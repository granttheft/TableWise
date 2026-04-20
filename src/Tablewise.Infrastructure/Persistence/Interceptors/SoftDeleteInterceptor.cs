using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tablewise.Domain.Common;

namespace Tablewise.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Soft Delete interceptor. EntityState.Deleted yerine IsDeleted=true set eder.
/// </summary>
public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// SaveChanges öncesi çağrılır.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateSoftDeleteStatuses(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// SaveChangesAsync öncesi çağrılır.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateSoftDeleteStatuses(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// EntityState.Deleted olan entity'leri Modified + IsDeleted=true olarak işaretler.
    /// </summary>
    private static void UpdateSoftDeleteStatuses(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
