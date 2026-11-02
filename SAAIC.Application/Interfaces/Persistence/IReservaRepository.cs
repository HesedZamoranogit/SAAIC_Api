using SAAIC.Application.Common.Models;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.Interfaces.Persistence;

public interface IReservaRepository : IRepository<Reserva>
{
    /// <summary>
    /// Verifica si existe al menos una instancia diaria confirmada que se solape en fecha y horario para el espacio dado.
    /// </summary>
    Task<bool> ExisteConflictoHorarioAsync(
        int idEspacio,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        int? excluirIdReserva = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica un conjunto de fechas de una sola vez (para reservas PorJornada de varios días o RecurrenteSemestral)
    /// y devuelve la lista de fechas que ya están ocupadas en ese horario.
    /// </summary>
    Task<IReadOnlyList<DateOnly>> ObtenerFechasConConflictoAsync(
        int idEspacio,
        IEnumerable<DateOnly> fechasAValidar,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una reserva por su Id incluyendo Espacio, Campus, Usuario, Asistencia e instancias Hijas (con sus Asistencias).
    /// </summary>
    Task<Reserva?> ObtenerConDetallesPorIdAsync(int idReserva, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las instancias diarias de eventos para una fecha o rango de fechas (para calendario y Reporte 1 de Eventos).
    /// </summary>
    Task<IReadOnlyList<Reserva>> ObtenerInstanciasPorRangoFechasAsync(
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int? idCampus = null,
        int? idEspacio = null,
        bool soloConfirmadas = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las reservaciones de un profesor específico (tanto reservas de 1 día como cabeceras y sus instancias hijas).
    /// </summary>
    Task<IReadOnlyList<Reserva>> ObtenerPorUsuarioAsync(
        string idUsuario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las reservaciones de un profesor de forma paginada.
    /// </summary>
    Task<PagedResult<Reserva>> ObtenerPorUsuarioPaginadoAsync(
        string idUsuario,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las reservaciones aplicando filtros opcionales de forma paginada.
    /// </summary>
    Task<PagedResult<Reserva>> ObtenerTodasPaginadoAsync(
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoReserva? estado = null,
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default);
}
