using Microsoft.EntityFrameworkCore;
using SAAIC.Application.Common.Models;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Infrastructure.Persistence.Context;

namespace SAAIC.Infrastructure.Persistence.Repositories;

public class EspacioRepository : Repository<Espacio>, IEspacioRepository
{
    public EspacioRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Espacio?> ObtenerConDetallesPorIdAsync(int idEspacio, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Campus)
            .Include(e => e.TipoEspacio)
            .FirstOrDefaultAsync(e => e.Id == idEspacio && e.Active, cancellationToken);
    }

    public async Task<Espacio?> ObtenerConBloqueoPesimistaAsync(int idEspacio, CancellationToken cancellationToken = default)
    {
        // Bloqueo pesimista a nivel de fila (UPDLOCK, ROWLOCK) en SQL Server:
        // Si dos profesores intentan reservar el mismo espacio en el mismo milisegundo,
        // el segundo espera en esta línea hasta que la transacción del primero haga Commit.
        return await _dbSet
            .FromSqlInterpolated($"SELECT * FROM Espacios WITH (UPDLOCK, ROWLOCK) WHERE Id = {idEspacio} AND Active = 1")
            .Include(e => e.Campus)
            .Include(e => e.TipoEspacio)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Espacio>> ObtenerConFiltrosAsync(
        int? idCampus = null,
        int? idTipoEspacio = null,
        EstadoEspacio? estado = null,
        bool soloActivos = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(e => e.Campus)
            .Include(e => e.TipoEspacio)
            .AsQueryable();

        if (soloActivos)
            query = query.Where(e => e.Active);

        if (idCampus.HasValue)
            query = query.Where(e => e.IdCampus == idCampus.Value);

        if (idTipoEspacio.HasValue)
            query = query.Where(e => e.IdTipoEspacio == idTipoEspacio.Value);

        if (estado.HasValue)
            query = query.Where(e => e.Estado == estado.Value);

        return await query
            .OrderBy(e => e.IdCampus)
            .ThenBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Espacio>> ObtenerConFiltrosPaginadoAsync(
        int? idCampus = null,
        int? idTipoEspacio = null,
        EstadoEspacio? estado = null,
        bool soloActivos = true,
        int pageNumber = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(e => e.Campus)
            .Include(e => e.TipoEspacio)
            .AsQueryable();

        if (soloActivos)
            query = query.Where(e => e.Active);

        if (idCampus.HasValue)
            query = query.Where(e => e.IdCampus == idCampus.Value);

        if (idTipoEspacio.HasValue)
            query = query.Where(e => e.IdTipoEspacio == idTipoEspacio.Value);

        if (estado.HasValue)
            query = query.Where(e => e.Estado == estado.Value);

        query = query.OrderBy(e => e.IdCampus).ThenBy(e => e.Nombre);

        return await query.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
    }
}
