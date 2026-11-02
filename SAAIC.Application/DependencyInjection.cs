using Microsoft.Extensions.DependencyInjection;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Application.Services;

namespace SAAIC.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de casos de uso y lógica de negocio de la capa de Aplicación.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IEspacioService, EspacioService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<IAsistenciaService, AsistenciaService>();
        services.AddScoped<IReporteService, ReporteService>();

        return services;
    }
}
