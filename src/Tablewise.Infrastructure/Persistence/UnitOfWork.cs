using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence.Repositories;

namespace Tablewise.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation. Transaction yönetimi ve repository erişimi sağlar.
/// </summary>
internal class UnitOfWork : IUnitOfWork
{
    private readonly TablewiseDbContext _context;
    private bool _disposed;

    // Repository lazy instances
    private IRepository<Tenant>? _tenants;
    private IRepository<User>? _users;
    private IRepository<UserInvitation>? _userInvitations;
    private IRepository<Venue>? _venues;
    private IRepository<VenueClosure>? _venueClosures;
    private IRepository<VenueCustomField>? _venueCustomFields;
    private IRepository<Table>? _tables;
    private IRepository<TableCombination>? _tableCombinations;
    private IRepository<Customer>? _customers;
    private IRepository<Reservation>? _reservations;
    private IRepository<AppliedRule>? _appliedRules;
    private IRepository<ReservationStatusLog>? _reservationStatusLogs;
    private IRepository<Rule>? _rules;
    private IRepository<Plan>? _plans;
    private IRepository<Subscription>? _subscriptions;
    private IRepository<NotificationLog>? _notificationLogs;
    private IRepository<AuditLog>? _auditLogs;
    private IRepository<IdempotencyKey>? _idempotencyKeys;

    /// <summary>
    /// UnitOfWork constructor.
    /// </summary>
    /// <param name="context">TablewiseDbContext instance</param>
    public UnitOfWork(TablewiseDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public IRepository<Tenant> Tenants => _tenants ??= new GenericRepository<Tenant>(_context);

    /// <inheritdoc />
    public IRepository<User> Users => _users ??= new GenericRepository<User>(_context);

    /// <inheritdoc />
    public IRepository<UserInvitation> UserInvitations => _userInvitations ??= new GenericRepository<UserInvitation>(_context);

    /// <inheritdoc />
    public IRepository<Venue> Venues => _venues ??= new GenericRepository<Venue>(_context);

    /// <inheritdoc />
    public IRepository<VenueClosure> VenueClosures => _venueClosures ??= new GenericRepository<VenueClosure>(_context);

    /// <inheritdoc />
    public IRepository<VenueCustomField> VenueCustomFields => _venueCustomFields ??= new GenericRepository<VenueCustomField>(_context);

    /// <inheritdoc />
    public IRepository<Table> Tables => _tables ??= new GenericRepository<Table>(_context);

    /// <inheritdoc />
    public IRepository<TableCombination> TableCombinations => _tableCombinations ??= new GenericRepository<TableCombination>(_context);

    /// <inheritdoc />
    public IRepository<Customer> Customers => _customers ??= new GenericRepository<Customer>(_context);

    /// <inheritdoc />
    public IRepository<Reservation> Reservations => _reservations ??= new GenericRepository<Reservation>(_context);

    /// <inheritdoc />
    public IRepository<AppliedRule> AppliedRules => _appliedRules ??= new GenericRepository<AppliedRule>(_context);

    /// <inheritdoc />
    public IRepository<ReservationStatusLog> ReservationStatusLogs => _reservationStatusLogs ??= new GenericRepository<ReservationStatusLog>(_context);

    /// <inheritdoc />
    public IRepository<Rule> Rules => _rules ??= new GenericRepository<Rule>(_context);

    /// <inheritdoc />
    public IRepository<Plan> Plans => _plans ??= new GenericRepository<Plan>(_context);

    /// <inheritdoc />
    public IRepository<Subscription> Subscriptions => _subscriptions ??= new GenericRepository<Subscription>(_context);

    /// <inheritdoc />
    public IRepository<NotificationLog> NotificationLogs => _notificationLogs ??= new GenericRepository<NotificationLog>(_context);

    /// <inheritdoc />
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(_context);

    /// <inheritdoc />
    public IRepository<IdempotencyKey> IdempotencyKeys => _idempotencyKeys ??= new GenericRepository<IdempotencyKey>(_context);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new UnitOfWorkTransaction(transaction);
    }

    /// <summary>
    /// Dispose implementation.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose pattern implementation.
    /// </summary>
    /// <param name="disposing">Disposing flag</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}

/// <summary>
/// Transaction wrapper implementation. Dispose edildiğinde rollback yapar.
/// </summary>
internal class UnitOfWorkTransaction : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _disposed;

    /// <summary>
    /// UnitOfWorkTransaction constructor.
    /// </summary>
    /// <param name="transaction">EF Core transaction</param>
    public UnitOfWorkTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    /// <summary>
    /// Dispose implementation. Rollback yapar.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose pattern implementation.
    /// </summary>
    /// <param name="disposing">Disposing flag</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction.Dispose();
        }
        _disposed = true;
    }
}
