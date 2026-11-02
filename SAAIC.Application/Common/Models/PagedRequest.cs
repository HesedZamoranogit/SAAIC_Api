namespace SAAIC.Application.Common.Models;

/// <summary>
/// Base de solicitud reutilizable para endpoints de listado con paginación.
/// El valor por defecto es 10 registros por página; un <see cref="PageSize"/> de 0 o menor solicita todos los registros ("Todos").
/// </summary>
public class PagedRequest
{
    /// <summary>
    /// Número de página solicitada (base 1). Por defecto 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página. Por defecto 10. Un valor de 0 o menor devuelve todos los registros ("Todos").
    /// </summary>
    public int? PageSize { get; set; } = 10;
}
