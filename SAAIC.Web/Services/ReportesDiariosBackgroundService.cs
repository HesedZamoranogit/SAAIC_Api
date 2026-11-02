using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Interfaces.Services;

namespace SAAIC.Web.Services;

/// <summary>
/// Servicio en segundo plano que marca inasistencias de eventos concluidos y envía automáticamente
/// por correo (vía API de Gmail) los reportes diarios en Excel en las horas configuradas.
/// </summary>
public class ReportesDiariosBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportesDiariosBackgroundService> _logger;
    private static readonly TimeSpan IntervaloRevision = TimeSpan.FromMinutes(5);

    public ReportesDiariosBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReportesDiariosBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Espera breve al arrancar la aplicación para permitir que terminen las migraciones y el seeder
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarCicloDiarioAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el ciclo del servicio automático de asistencia y reportes diarios.");
            }

            await Task.Delay(IntervaloRevision, stoppingToken);
        }
    }

    private async Task ProcesarCicloDiarioAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var configRepo = scope.ServiceProvider.GetRequiredService<IConfiguracionSistemaRepository>();
        var asistenciaService = scope.ServiceProvider.GetRequiredService<IAsistenciaService>();
        var reporteService = scope.ServiceProvider.GetRequiredService<IReporteService>();

        var ahora = DateTime.Now;
        var fechaHoy = DateOnly.FromDateTime(ahora);
        var horaActual = TimeOnly.FromDateTime(ahora);

        // 1. Marcar como Inasistencia los eventos del día que ya concluyeron sin registro de entrada
        var inasistenciasMarcadas = await asistenciaService.ProcesarInasistenciasVencidasAsync(
            fechaHoy, horaActual, cancellationToken);

        if (inasistenciasMarcadas > 0)
        {
            _logger.LogInformation(
                "Se marcaron automáticamente {Cantidad} inasistencias para eventos concluidos el {Fecha}.",
                inasistenciasMarcadas, fechaHoy);
        }

        // 2. Verificar envío de reportes automáticos diarios
        var config = await configRepo.ObtenerConfiguracionActivaAsync(cancellationToken);
        if (!config.EnvioAutomaticoReportesActivo)
            return;

        if (horaActual >= config.HoraEnvioReporteEventos)
        {
            await reporteService.EjecutarReporteAutomaticoEventosDelDiaAsync(fechaHoy, cancellationToken);
        }

        if (horaActual >= config.HoraEnvioReporteAsistencia)
        {
            await reporteService.EjecutarReporteAutomaticoAsistenciaDelDiaAsync(fechaHoy, cancellationToken);
        }
    }
}
