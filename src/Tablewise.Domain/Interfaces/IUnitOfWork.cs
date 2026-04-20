using Tablewise.Domain.Entities;

namespace Tablewise.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern. Transaction yönetimi ve repository erişimi sağlar.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Tenant repository.
    /// </summary>
    IRepository<Tenant> Tenants { get; }

    /// <summary>
    /// User repository.
    /// </summary>
    IRepository<User> Users { get; }

    /// <summary>
    /// UserInvitation repository.
    /// </summary>
    IRepository<UserInvitation> UserInvitations { get; }

    /// <summary>
    /// Venue repository.
    /// </summary>
    IRepository<Venue> Venues { get; }

    /// <summary>
    /// VenueClosure repository.
    /// </summary>
    IRepository<VenueClosure> VenueClosures { get; }

    /// <summary>
    /// VenueCustomField repository.
    /// </summary>
    IRepository<VenueCustomField> VenueCustomFields { get; }

    /// <summary>
    /// Table repository.
    /// </summary>
    IRepository<Table> Tables { get; }

    /// <summary>
    /// TableCombination repository.
    /// </summary>
    IRepository<TableCombination> TableCombinations { get; }

    /// <summary>
    /// Customer repository.
    /// </summary>
    IRepository<Customer> Customers { get; }

    /// <summary>
    /// Reservation repository.
    /// </summary>
    IRepository<Reservation> Reservations { get; }

    /// <summary>
    /// AppliedRule repository.
    /// </summary>
    IRepository<AppliedRule> AppliedRules { get; }

    /// <summary>
    /// ReservationStatusLog repository.
    /// </summary>
    IRepository<ReservationStatusLog> ReservationStatusLogs { get; }

    /// <summary>
    /// Rule repository.
    /// </summary>
    IRepository<Rule> Rules { get; }

    /// <summary>
    /// Plan repository.
    /// </summary>
    IRepository<Plan> Plans { get; }

    /// <summary>
    /// Subscription repository.
    /// </summary>
    IRepository<Subscription> Subscriptions { get; }

    /// <summary>
    /// NotificationLog repository.
    /// </summary>
    IRepository<NotificationLog> NotificationLogs { get; }

    /// <summary>
    /// AuditLog repository.
    /// </summary>
    IRepository<AuditLog> AuditLogs { get; }

    /// <summary>
    /// IdempotencyKey repository.
    /// </summary>
    IRepository<IdempotencyKey> IdempotencyKeys { get; }

    /// <summary>
    /// Tüm değişiklikleri veritabanına kaydeder.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Etkilenen kayıt sayısı</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction başlatır. Dispose edildiğinde rollback yapar, commit için CommitAsync çağrılmalı.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction</returns>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction wrapper. Dispose edildiğinde rollback yapar.
/// </summary>
public interface IUnitOfWorkTransaction : IDisposable
{
    /// <summary>
    /// Transaction'ı commit eder.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction'ı rollback eder.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
