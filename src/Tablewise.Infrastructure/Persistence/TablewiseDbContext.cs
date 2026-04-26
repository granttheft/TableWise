using Microsoft.EntityFrameworkCore;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Common;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Interfaces;
using Tablewise.Infrastructure.Persistence.Interceptors;

namespace Tablewise.Infrastructure.Persistence;

/// <summary>
/// Tablewise uygulaması için ana DbContext.
/// Multi-tenant yapı, Global Query Filter ve Soft Delete desteği içerir.
/// IApplicationDbContext implementasyonu ile Clean Architecture sağlanır.
/// </summary>
public class TablewiseDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// TablewiseDbContext constructor.
    /// </summary>
    /// <param name="options">DbContext options</param>
    /// <param name="tenantContext">Tenant context</param>
    /// <param name="currentUser">Current user</param>
    public TablewiseDbContext(
        DbContextOptions<TablewiseDbContext> options,
        ITenantContext tenantContext,
        ICurrentUser currentUser) : base(options)
    {
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    // DbSets
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<VenueClosure> VenueClosures => Set<VenueClosure>();
    public DbSet<VenueCustomField> VenueCustomFields => Set<VenueCustomField>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<TableCombination> TableCombinations => Set<TableCombination>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<AppliedRule> AppliedRules => Set<AppliedRule>();
    public DbSet<ReservationStatusLog> ReservationStatusLogs => Set<ReservationStatusLog>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<RevocableRefreshToken> RefreshTokens => Set<RevocableRefreshToken>();

    /// <summary>
    /// Model yapılandırması. Assembly'den tüm IEntityTypeConfiguration'ları uygular.
    /// Global Query Filter ve index'leri tanımlar.
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Assembly'den tüm IEntityTypeConfiguration'ları uygula
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TablewiseDbContext).Assembly);

        // Global Query Filter - TenantScopedEntity için
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // TenantScopedEntity'den türeyen entity'ler için TenantId ve Soft Delete filter
            if (typeof(TenantScopedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var tenantIdProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(TenantScopedEntity.TenantId));
                var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(TenantScopedEntity.IsDeleted));

                var tenantIdEquals = System.Linq.Expressions.Expression.Equal(
                    tenantIdProperty,
                    System.Linq.Expressions.Expression.Constant(_tenantContext.TenantId));

                var notDeleted = System.Linq.Expressions.Expression.Equal(
                    isDeletedProperty,
                    System.Linq.Expressions.Expression.Constant(false));

                var combinedFilter = System.Linq.Expressions.Expression.AndAlso(tenantIdEquals, notDeleted);
                var lambda = System.Linq.Expressions.Expression.Lambda(combinedFilter, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            // BaseEntity'den türeyen ama TenantScopedEntity olmayan (Plan gibi) - sadece Soft Delete
            else if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) &&
                     entityType.ClrType != typeof(TenantScopedEntity))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));

                var notDeleted = System.Linq.Expressions.Expression.Equal(
                    isDeletedProperty,
                    System.Linq.Expressions.Expression.Constant(false));

                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // DateTime UTC converter - tüm DateTime property'ler için
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }
    }

    /// <summary>
    /// SaveChangesAsync override. CreatedAt/UpdatedAt otomatik set, Soft Delete desteği.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // CreatedAt, UpdatedAt ve TenantId otomatik set
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;

                    // TenantScopedEntity için TenantId otomatik set (eğer henüz set edilmemişse)
                    if (entry.Entity is TenantScopedEntity tenantScopedEntity && tenantScopedEntity.TenantId == Guid.Empty)
                    {
                        // TenantContext'ten TenantId al (seed sırasında null olabilir, o zaman atlıyoruz)
                        try
                        {
                            tenantScopedEntity.TenantId = _tenantContext.TenantId;
                        }
                        catch (InvalidOperationException)
                        {
                            // TenantContext set edilmemişse (seed, migration gibi durumlarda),
                            // entity'nin TenantId'si zaten manuel set edilmiş olmalı
                        }
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
