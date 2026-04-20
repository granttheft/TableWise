using System.Linq.Expressions;
using Tablewise.Domain.Common;

namespace Tablewise.Domain.Interfaces;

/// <summary>
/// Generic repository interface. BaseEntity türeyen tüm entity'ler için CRUD operasyonları sağlar.
/// </summary>
/// <typeparam name="T">BaseEntity'den türeyen entity tipi</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// ID'ye göre tekil entity getirir.
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity veya null</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşullara göre entity listesi getirir. Filtreleme, sıralama, sayfalama destekler.
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu (opsiyonel)</param>
    /// <param name="orderBy">Sıralama fonksiyonu (opsiyonel)</param>
    /// <param name="includeProperties">Yüklenecek ilişkili entity'ler (virgülle ayrılmış)</param>
    /// <param name="skip">Kaç kayıt atlanacak (sayfalama için)</param>
    /// <param name="take">Kaç kayıt alınacak (sayfalama için)</param>
    /// <param name="asNoTracking">AsNoTracking kullanılsın mı (default true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity listesi</returns>
    Task<IReadOnlyList<T>> GetAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeProperties = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula uyan ilk entity'yi getirir.
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu</param>
    /// <param name="includeProperties">Yüklenecek ilişkili entity'ler (virgülle ayrılmış)</param>
    /// <param name="asNoTracking">AsNoTracking kullanılsın mı (default true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity veya null</returns>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        string? includeProperties = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni entity ekler. SaveChangesAsync çağrılmalıdır.
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Var olan entity'yi günceller. SaveChangesAsync çağrılmalıdır.
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    void Update(T entity);

    /// <summary>
    /// Entity'yi siler (soft delete). SaveChangesAsync çağrılmalıdır.
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    void Remove(T entity);

    /// <summary>
    /// Koşula uyan entity var mı kontrol eder.
    /// </summary>
    /// <param name="predicate">Kontrol koşulu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Varsa true</returns>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Koşula uyan entity sayısını döner.
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu (opsiyonel)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Kayıt sayısı</returns>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Karmaşık sorgular için IQueryable döner. DİKKATLE kullanılmalı!
    /// Global Query Filter otomatik uygulanır.
    /// </summary>
    /// <returns>IQueryable</returns>
    IQueryable<T> Query();
}
