namespace SAAIC.Application.Common.Models;

/// <summary>
/// Representa un bloque paginado de resultados junto con los metadatos de paginación.
/// </summary>
/// <typeparam name="T">Tipo de los elementos contenidos en la página.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Elementos correspondientes a la página solicitada.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Número de página actual (base 1).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Cantidad de registros solicitados por página. Cuando se solicitan todos los registros refleja el total.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de registros que cumplen los filtros aplicados.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Total de páginas calculado a partir del tamaño de página y el total de registros.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica si existe una página anterior a la actual.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica si existe una página posterior a la actual.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
