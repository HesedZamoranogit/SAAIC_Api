using SAAIC.Application.Common.Models;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.Interfaces.Persistence;

public interface IEspacioRepository : IRepository<Espacio>
{
    /// <summary>
    /// Obtiene un espacio incluyendo su Campus y TipoEspacio.
    /// </summary>
    Task<Espacio?> ObtenerConDetallesPorIdAsync(int idEspacio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adquiere un bloqueo pesimista a nivel de fila (UPDLOCK, ROWLOCK en SQL Server) sobre el espacio
    /// dentro de la transacción actual para impedir que dos usuarios reserven el mismo espacio en el mismo milisegundo.
    /// </summary>
    Task<Espacio?> ObtenerConBloqueoPesimistaAsync(int idEspacio, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista espacios filtrando opcionalmente por campus, tipo de espacio y estado.
    /// </summary>
    Task<IReadOnlyList<Espacio>> ObtenerConFiltrosAsync(
        int? idCampus = null,
        int? idTipoEspacio = null,
        EstadoEspacio? estado = null,
        bool soloActivos = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista espacios de forma paginada aplicando filtros opcionales.
    /// </summary>
    Task<PagedResult<Espacio>> ObtenerConFiltrosPaginadoAsync(
        int? idCampus = null,
        int? idTipoEspacio = null,
        EstadoEspacio? estado = null,
        bool soloActivos = true,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default);
}
