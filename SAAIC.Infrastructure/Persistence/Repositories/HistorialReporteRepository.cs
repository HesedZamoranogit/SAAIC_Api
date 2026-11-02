using Microsoft.EntityFrameworkCore;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Infrastructure.Persistence.Context;

namespace SAAIC.Infrastructure.Persistence.Repositories;

public class HistorialReporteRepository : Repository<HistorialReporte>, IHistorialReporteRepository
{
    public HistorialReporteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> YaSeEnvioReporteAutomaticoAsync(
        TipoReporte tipoReporte,
        DateOnly fechaDia,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(h =>
                h.Active &&
                h.TipoReporte == tipoReporte &&
                h.Origen == OrigenReporte.AutomaticoDiario &&
                h.FechaInicioReporte == fechaDia &&
                h.EstadoEnvio == EstadoEnvioReporte.EnviadoPorCorreo,
                cancellationToken);
    }
}
