using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tablewise.Domain.Common;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation. BaseEntity türeyen tüm entity'ler için CRUD operasyonları sağlar.
/// Global Query Filter ve Soft Delete otomatik uygulanır.
/// </summary>
/// <typeparam name="T">BaseEntity'den türeyen entity tipi</typeparam>
internal class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly TablewiseDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// GenericRepository constructor.
    /// </summary>
    /// <param name="context">DbContext instance</param>
    public GenericRepository(TablewiseDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Include ilişkili entity'ler
        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        // Filtreleme
        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        // Sıralama
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Sayfalama
        if (skip.HasValue && skip.Value > 0)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue && take.Value > 0)
        {
            query = query.Take(take.Value);
        }

        // AsNoTracking
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string? includeProperties = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        // Include ilişkili entity'ler
        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        // AsNoTracking
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public void Remove(T entity)
    {
        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
