using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;

namespace SAAIC.Application.Interfaces.Persistence;

public interface IHistorialReporteRepository : IRepository<HistorialReporte>
{
    /// <summary>
    /// Verifica si el reporte automático diario de un tipo específico ya fue enviado con éxito para la fecha dada.
    /// </summary>
    Task<bool> YaSeEnvioReporteAutomaticoAsync(
        TipoReporte tipoReporte,
        DateOnly fechaDia,
        CancellationToken cancellationToken = default);
}
