using Microsoft.EntityFrameworkCore;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Domain.Entities;
using SAAIC.Infrastructure.Persistence.Context;

namespace SAAIC.Infrastructure.Persistence.Repositories;

public class ConfiguracionSistemaRepository : Repository<ConfiguracionSistema>, IConfiguracionSistemaRepository
{
    public ConfiguracionSistemaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ConfiguracionSistema> ObtenerConfiguracionActivaAsync(CancellationToken cancellationToken = default)
    {
        var config = await _dbSet
            .FirstOrDefaultAsync(c => c.Active, cancellationToken);

        if (config != null)
            return config;

        config = new ConfiguracionSistema
        {
            MinutosToleranciaRetardo = 15,
            MinutosAnticipacionCheckIn = 30,
            EnvioAutomaticoReportesActivo = true,
            HoraEnvioReporteEventos = new TimeOnly(7, 0),
            HoraEnvioReporteAsistencia = new TimeOnly(22, 0),
            CreatedBy = "SYSTEM"
        };

        await _dbSet.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return config;
    }
}
