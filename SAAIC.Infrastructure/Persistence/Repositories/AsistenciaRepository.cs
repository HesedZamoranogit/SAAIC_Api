using Microsoft.EntityFrameworkCore;
using SAAIC.Application.Common.Models;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Infrastructure.Persistence.Context;

namespace SAAIC.Infrastructure.Persistence.Repositories;

public class AsistenciaRepository : Repository<Asistencia>, IAsistenciaRepository
{
    public AsistenciaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Asistencia?> ObtenerPorIdReservaAsync(int idReserva, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Espacio)
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.IdReserva == idReserva && a.Active, cancellationToken);
    }

    public async Task<IReadOnlyList<Asistencia>> ObtenerPendientesVencidasAsync(
        DateOnly fecha,
        TimeOnly horaCorteActual,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Reserva)
            .Where(a =>
                a.Active &&
                a.FechaEvento == fecha &&
                a.Estado == EstadoAsistencia.Pendiente &&
                a.HoraProgramadaFin <= horaCorteActual &&
                a.Reserva != null &&
                a.Reserva.Estado == EstadoReserva.Confirmada)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Asistencia>> ObtenerParaReporteConFiltrosAsync(
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoAsistencia? estado = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(a => a.Usuario)
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Espacio)
                    .ThenInclude(e => e!.TipoEspacio)
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Campus)
            .Where(a =>
                a.Active &&
                a.FechaEvento >= fechaInicio &&
                a.FechaEvento <= fechaFin &&
                a.Reserva != null &&
                a.Reserva.Estado == EstadoReserva.Confirmada);

        if (idCampus.HasValue)
            query = query.Where(a => a.Reserva!.IdCampus == idCampus.Value);

        if (idEspacio.HasValue)
            query = query.Where(a => a.Reserva!.IdEspacio == idEspacio.Value);

        if (!string.IsNullOrWhiteSpace(idUsuario))
            query = query.Where(a => a.IdUsuario == idUsuario);

        if (estado.HasValue)
            query = query.Where(a => a.Estado == estado.Value);

        return await query
            .OrderBy(a => a.FechaEvento)
            .ThenBy(a => a.HoraProgramadaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Asistencia>> ObtenerPaginadoConFiltrosAsync(
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuario = null,
        EstadoAsistencia? estado = null,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(a => a.Usuario)
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Espacio)
                    .ThenInclude(e => e!.TipoEspacio)
            .Include(a => a.Reserva)
                .ThenInclude(r => r!.Campus)
            .Where(a => a.Active && a.Reserva != null && a.Reserva.Estado == EstadoReserva.Confirmada);

        if (fechaInicio.HasValue)
            query = query.Where(a => a.FechaEvento >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(a => a.FechaEvento <= fechaFin.Value);

        if (idCampus.HasValue)
            query = query.Where(a => a.Reserva!.IdCampus == idCampus.Value);

        if (idEspacio.HasValue)
            query = query.Where(a => a.Reserva!.IdEspacio == idEspacio.Value);

        if (!string.IsNullOrWhiteSpace(idUsuario))
            query = query.Where(a => a.IdUsuario == idUsuario);

        if (estado.HasValue)
            query = query.Where(a => a.Estado == estado.Value);

        query = query
            .OrderByDescending(a => a.FechaEvento)
            .ThenBy(a => a.HoraProgramadaInicio);

        return await query.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
    }
}
