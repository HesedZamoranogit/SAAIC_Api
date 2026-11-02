using Microsoft.EntityFrameworkCore;
using SAAIC.Application.Common.Models;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Infrastructure.Persistence.Context;

namespace SAAIC.Infrastructure.Persistence.Repositories;

public class ReservaRepository : Repository<Reserva>, IReservaRepository
{
    public ReservaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExisteConflictoHorarioAsync(
        int idEspacio,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        int? excluirIdReserva = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r =>
            r.Active &&
            !r.EsReservaPadre &&
            r.IdEspacio == idEspacio &&
            r.FechaInicio == fecha &&
            r.Estado == EstadoReserva.Confirmada &&
            r.HoraInicio < horaFin &&
            r.HoraFin > horaInicio);

        if (excluirIdReserva.HasValue)
        {
            query = query.Where(r => r.Id != excluirIdReserva.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DateOnly>> ObtenerFechasConConflictoAsync(
        int idEspacio,
        IEnumerable<DateOnly> fechasAValidar,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        CancellationToken cancellationToken = default)
    {
        var listaFechas = fechasAValidar.Distinct().ToList();
        if (listaFechas.Count == 0)
            return Array.Empty<DateOnly>();

        return await _dbSet
            .AsNoTracking()
            .Where(r =>
                r.Active &&
                !r.EsReservaPadre &&
                r.IdEspacio == idEspacio &&
                r.Estado == EstadoReserva.Confirmada &&
                listaFechas.Contains(r.FechaInicio) &&
                r.HoraInicio < horaFin &&
                r.HoraFin > horaInicio)
            .Select(r => r.FechaInicio)
            .Distinct()
            .OrderBy(f => f)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reserva?> ObtenerConDetallesPorIdAsync(int idReserva, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Espacio)
                .ThenInclude(e => e!.TipoEspacio)
            .Include(r => r.Campus)
            .Include(r => r.Usuario)
            .Include(r => r.Asistencia)
            .Include(r => r.ReservasHijas)
                .ThenInclude(h => h.Asistencia)
            .FirstOrDefaultAsync(r => r.Id == idReserva && r.Active, cancellationToken);
    }

    public async Task<IReadOnlyList<Reserva>> ObtenerInstanciasPorRangoFechasAsync(
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int? idCampus = null,
        int? idEspacio = null,
        bool soloConfirmadas = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(r => r.Espacio)
                .ThenInclude(e => e!.TipoEspacio)
            .Include(r => r.Campus)
            .Include(r => r.Usuario)
            .Include(r => r.Asistencia)
            .Where(r =>
                r.Active &&
                !r.EsReservaPadre &&
                r.FechaInicio >= fechaInicio &&
                r.FechaInicio <= fechaFin);

        if (soloConfirmadas)
            query = query.Where(r => r.Estado == EstadoReserva.Confirmada);

        if (idCampus.HasValue)
            query = query.Where(r => r.IdCampus == idCampus.Value);

        if (idEspacio.HasValue)
            query = query.Where(r => r.IdEspacio == idEspacio.Value);

        return await query
            .OrderBy(r => r.FechaInicio)
            .ThenBy(r => r.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reserva>> ObtenerPorUsuarioAsync(
        string idUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Espacio)
                .ThenInclude(e => e!.TipoEspacio)
            .Include(r => r.Campus)
            .Include(r => r.Asistencia)
            .Include(r => r.ReservasHijas)
                .ThenInclude(h => h.Asistencia)
            .Where(r => r.Active && r.IdUsuario == idUsuario && r.IdReservaPadre == null)
            .OrderByDescending(r => r.FechaInicio)
            .ThenBy(r => r.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Reserva>> ObtenerPorUsuarioPaginadoAsync(
        string idUsuario,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Espacio)
                .ThenInclude(e => e!.TipoEspacio)
            .Include(r => r.Campus)
            .Include(r => r.Asistencia)
            .Include(r => r.ReservasHijas)
                .ThenInclude(h => h.Asistencia)
            .Where(r => r.Active && r.IdUsuario == idUsuario && r.IdReservaPadre == null)
            .OrderByDescending(r => r.FechaInicio)
            .ThenBy(r => r.HoraInicio)
            .ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
    }

    public async Task<PagedResult<Reserva>> ObtenerTodasPaginadoAsync(
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoReserva? estado = null,
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(r => r.Espacio)
                .ThenInclude(e => e!.TipoEspacio)
            .Include(r => r.Campus)
            .Include(r => r.Usuario)
            .Include(r => r.Asistencia)
            .Include(r => r.ReservasHijas)
                .ThenInclude(h => h.Asistencia)
            .Where(r => r.Active && r.IdReservaPadre == null)
            .AsQueryable();

        if (idCampus.HasValue)
            query = query.Where(r => r.IdCampus == idCampus.Value);

        if (idEspacio.HasValue)
            query = query.Where(r => r.IdEspacio == idEspacio.Value);

        if (!string.IsNullOrWhiteSpace(idUsuario))
            query = query.Where(r => r.IdUsuario == idUsuario);

        if (estado.HasValue)
            query = query.Where(r => r.Estado == estado.Value);

        if (fechaInicio.HasValue)
            query = query.Where(r => r.FechaInicio >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(r => r.FechaFin <= fechaFin.Value);

        query = query
            .OrderByDescending(r => r.FechaInicio)
            .ThenBy(r => r.HoraInicio);

        return await query.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
    }
}
