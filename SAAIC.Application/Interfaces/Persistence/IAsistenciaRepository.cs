using SAAIC.Application.Common.Models;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.Interfaces.Persistence;

public interface IAsistenciaRepository : IRepository<Asistencia>
{
    /// <summary>
    /// Obtiene el registro de asistencia correspondiente a una instancia diaria de reserva.
    /// </summary>
    Task<Asistencia?> ObtenerPorIdReservaAsync(int idReserva, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las asistencias de una fecha cuyo evento ya terminó y siguen en estado Pendiente
    /// (para marcarlas como Inasistencia en el corte diario).
    /// </summary>
    Task<IReadOnlyList<Asistencia>> ObtenerPendientesVencidasAsync(
        DateOnly fecha,
        TimeOnly horaCorteActual,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los registros de asistencia con todos sus detalles (Reserva, Espacio, Campus, Usuario)
    /// aplicando filtros para el Reporte 2 de Asistencia (tanto diario automático como manual por Administradores).
    /// </summary>
    Task<IReadOnlyList<Asistencia>> ObtenerParaReporteConFiltrosAsync(
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoAsistencia? estado = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros de asistencia paginados aplicando filtros opcionales.
    /// </summary>
    Task<PagedResult<Asistencia>> ObtenerPaginadoConFiltrosAsync(
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoAsistencia? estado = null,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default);
}
