namespace Tablewise.Application.DTOs.Common;

/// <summary>
/// Sayfalı liste sonucu.
/// </summary>
/// <typeparam name="T">Liste öğesi tipi.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// Öğeler.
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Toplam kayıt sayısı.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Mevcut sayfa.
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Sayfa boyutu.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Toplam sayfa sayısı.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Önceki sayfa var mı?
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Sonraki sayfa var mı?
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
