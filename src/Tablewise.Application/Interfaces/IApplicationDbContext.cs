using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tablewise.Domain.Entities;

namespace Tablewise.Application.Interfaces;

/// <summary>
/// Application katmanı için DbContext soyutlaması.
/// Clean Architecture'a uygun olarak Infrastructure'a bağımlılık kaldırılır.
/// </summary>
public interface IApplicationDbContext
{
    // Entity DbSets
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<UserInvitation> UserInvitations { get; }
    DbSet<Venue> Venues { get; }
    DbSet<VenueClosure> VenueClosures { get; }
    DbSet<VenueCustomField> VenueCustomFields { get; }
    DbSet<Table> Tables { get; }
    DbSet<TableCombination> TableCombinations { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<AppliedRule> AppliedRules { get; }
    DbSet<ReservationStatusLog> ReservationStatusLogs { get; }
    DbSet<Rule> Rules { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<NotificationLog> NotificationLogs { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<IdempotencyKey> IdempotencyKeys { get; }
    DbSet<RevocableRefreshToken> RefreshTokens { get; }

    /// <summary>
    /// Değişiklikleri veritabanına kaydeder.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Change tracker - entity durumlarını izlemek için.
    /// </summary>
    ChangeTracker ChangeTracker { get; }

    /// <summary>
    /// Database facade - transaction, migration vb. işlemler için.
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// DbSet üzerinde generic erişim sağlar.
    /// </summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Entity'yi attach eder.
    /// </summary>
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}
