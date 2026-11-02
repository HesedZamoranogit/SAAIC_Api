using SAAIC.Domain.Entities;

namespace SAAIC.Application.Interfaces.Persistence;

public interface IConfiguracionSistemaRepository : IRepository<ConfiguracionSistema>
{
    /// <summary>
    /// Obtiene la configuración activa del sistema (tiempo de tolerancia para retardo, horarios de reportes, etc.).
    /// Si aún no existe en base de datos, devuelve los valores por defecto.
    /// </summary>
    Task<ConfiguracionSistema> ObtenerConfiguracionActivaAsync(CancellationToken cancellationToken = default);
}
