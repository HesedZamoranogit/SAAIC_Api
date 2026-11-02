using Microsoft.EntityFrameworkCore;

namespace SAAIC.Application.Common.Models;

/// <summary>
/// Utilidades reutilizables para aplicar paginación de forma estándar en las consultas de listado.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Aplica paginación sobre una consulta EF Core y devuelve el bloque solicitado junto con sus metadatos.
    /// Un <paramref name="pageSize"/> nulo o menor o igual a cero devuelve todos los registros ("Todos").
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos de la consulta.</typeparam>
    /// <param name="query">Consulta filtrada a paginar.</param>
    /// <param name="pageNumber">Número de página solicitada (base 1).</param>
    /// <param name="pageSize">Tamaño de página. Nulo o menor o igual a cero devuelve todos los registros.</param>
    /// <param name="ct">Token de cancelación.</param>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int? pageSize,
        CancellationToken ct = default)
    {
        var totalRecords = await query.CountAsync(ct);

        var normalizedPageNumber = pageNumber < 1 ? 1 : pageNumber;
        var returnAllRecords = !pageSize.HasValue || pageSize.Value <= 0;

        var items = returnAllRecords
            ? await query.ToListAsync(ct)
            : await query
                .Skip((normalizedPageNumber - 1) * pageSize!.Value)
                .Take(pageSize.Value)
                .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = returnAllRecords ? totalRecords : pageSize!.Value,
            TotalRecords = totalRecords,
            TotalPages = returnAllRecords
                ? (totalRecords == 0 ? 0 : 1)
                : (int)Math.Ceiling(totalRecords / (double)pageSize!.Value)
        };
    }

    /// <summary>
    /// Aplica paginación sobre una lista ya materializada en memoria.
    /// Un <paramref name="pageSize"/> nulo o menor o igual a cero devuelve todos los registros ("Todos").
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos de la lista.</typeparam>
    /// <param name="source">Colección origen a paginar.</param>
    /// <param name="pageNumber">Número de página solicitada (base 1).</param>
    /// <param name="pageSize">Tamaño de página. Nulo o menor o igual a cero devuelve todos los registros.</param>
    public static PagedResult<T> ToPagedResult<T>(
        this IReadOnlyList<T> source,
        int pageNumber,
        int? pageSize)
    {
        var totalRecords = source.Count;

        var normalizedPageNumber = pageNumber < 1 ? 1 : pageNumber;
        var returnAllRecords = !pageSize.HasValue || pageSize.Value <= 0;

        var items = returnAllRecords
            ? source
            : (IReadOnlyList<T>)source
                .Skip((normalizedPageNumber - 1) * pageSize!.Value)
                .Take(pageSize.Value)
                .ToList();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = returnAllRecords ? totalRecords : pageSize!.Value,
            TotalRecords = totalRecords,
            TotalPages = returnAllRecords
                ? (totalRecords == 0 ? 0 : 1)
                : (int)Math.Ceiling(totalRecords / (double)pageSize!.Value)
        };
    }

    /// <summary>
    /// Transforma los elementos de un PagedResult a otro tipo proyectando cada elemento (por ejemplo de Entidad a DTO).
    /// </summary>
    public static PagedResult<TDestination> MapTo<TSource, TDestination>(
        this PagedResult<TSource> source,
        Func<TSource, TDestination> mapFunc)
    {
        return new PagedResult<TDestination>
        {
            Items = source.Items.Select(mapFunc).ToList(),
            PageNumber = source.PageNumber,
            PageSize = source.PageSize,
            TotalRecords = source.TotalRecords,
            TotalPages = source.TotalPages
        };
    }
}
