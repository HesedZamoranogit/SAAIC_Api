using SAAIC.Application.DTOs.Reportes;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Application.Services;

public class ReporteService : IReporteService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IEspacioRepository _espacioRepository;
    private readonly IAsistenciaRepository _asistenciaRepository;
    private readonly IConfiguracionSistemaRepository _configuracionRepository;
    private readonly IHistorialReporteRepository _historialReporteRepository;
    private readonly IAsistenciaService _asistenciaService;
    private readonly IExcelReportGenerator _excelReportGenerator;
    private readonly IEmailService _emailService;

    public ReporteService(
        IReservaRepository reservaRepository,
        IEspacioRepository espacioRepository,
        IAsistenciaRepository asistenciaRepository,
        IConfiguracionSistemaRepository configuracionRepository,
        IHistorialReporteRepository historialReporteRepository,
        IAsistenciaService asistenciaService,
        IExcelReportGenerator excelReportGenerator,
        IEmailService emailService)
    {
        _reservaRepository = reservaRepository;
        _espacioRepository = espacioRepository;
        _asistenciaRepository = asistenciaRepository;
        _configuracionRepository = configuracionRepository;
        _historialReporteRepository = historialReporteRepository;
        _asistenciaService = asistenciaService;
        _excelReportGenerator = excelReportGenerator;
        _emailService = emailService;
    }

    public async Task<ArchivoReporteDto> GenerarReporteEventosManualAsync(
        FiltroReporteEventosRequestDto filtro,
        string adminUsuarioId,
        CancellationToken cancellationToken = default)
    {
        if (filtro.FechaFin < filtro.FechaInicio)
            throw new DomainException("La fecha de fin no puede ser menor a la fecha de inicio.");

        var eventos = await _reservaRepository.ObtenerInstanciasPorRangoFechasAsync(
            filtro.FechaInicio,
            filtro.FechaFin,
            filtro.IdCampus,
            filtro.IdEspacio,
            soloConfirmadas: false,
            cancellationToken);

        var espacios = await _espacioRepository.ObtenerConFiltrosAsync(
            filtro.IdCampus,
            idTipoEspacio: null,
            estado: null,
            soloActivos: true,
            cancellationToken);

        var excelBytes = _excelReportGenerator.GenerarReporteEventosYEspacios(
            eventos,
            espacios,
            filtro.FechaInicio,
            filtro.FechaFin);

        var nombreArchivo = $"Reporte_Eventos_Espacios_{filtro.FechaInicio:yyyyMMdd}_{filtro.FechaFin:yyyyMMdd}.xlsx";

        if (!string.IsNullOrWhiteSpace(filtro.EnviarPorCorreoA))
        {
            var destinatarios = SepararCorreos(filtro.EnviarPorCorreoA);
            await _emailService.SendEmailWithAttachmentAsync(
                destinatarios,
                $"Reporte de Eventos y Espacios ({filtro.FechaInicio:yyyy-MM-dd} al {filtro.FechaFin:yyyy-MM-dd}) - ITSON",
                $"<p>Adjunto se encuentra el reporte en Excel de eventos y espacios generado manualmente del <strong>{filtro.FechaInicio:yyyy-MM-dd}</strong> al <strong>{filtro.FechaFin:yyyy-MM-dd}</strong>.</p>",
                excelBytes,
                nombreArchivo,
                cancellationToken: cancellationToken);
        }

        var registroHistorial = HistorialReporte.RegistrarReporteManual(
            TipoReporte.EventosYEspacios,
            filtro.FechaInicio,
            filtro.FechaFin,
            nombreArchivo,
            eventos.Count,
            adminUsuarioId,
            filtro.IdCampus,
            filtro.IdEspacio,
            idUsuarioFiltro: null,
            enviadoACorreo: filtro.EnviarPorCorreoA);

        await _historialReporteRepository.AddAsync(registroHistorial, cancellationToken);

        return new ArchivoReporteDto(excelBytes, nombreArchivo);
    }

    public async Task<ArchivoReporteDto> GenerarReporteAsistenciaManualAsync(
        FiltroReporteAsistenciaRequestDto filtro,
        string adminUsuarioId,
        CancellationToken cancellationToken = default)
    {
        if (filtro.FechaFin < filtro.FechaInicio)
            throw new DomainException("La fecha de fin no puede ser menor a la fecha de inicio.");

        var asistencias = await _asistenciaRepository.ObtenerParaReporteConFiltrosAsync(
            filtro.FechaInicio,
            filtro.FechaFin,
            filtro.IdCampus,
            filtro.IdEspacio,
            filtro.IdUsuario,
            filtro.EstadoAsistencia,
            cancellationToken);

        var excelBytes = _excelReportGenerator.GenerarReporteAsistencia(
            asistencias,
            filtro.FechaInicio,
            filtro.FechaFin);

        var nombreArchivo = $"Reporte_Asistencia_{filtro.FechaInicio:yyyyMMdd}_{filtro.FechaFin:yyyyMMdd}.xlsx";

        if (!string.IsNullOrWhiteSpace(filtro.EnviarPorCorreoA))
        {
            var destinatarios = SepararCorreos(filtro.EnviarPorCorreoA);
            await _emailService.SendEmailWithAttachmentAsync(
                destinatarios,
                $"Reporte de Asistencia ({filtro.FechaInicio:yyyy-MM-dd} al {filtro.FechaFin:yyyy-MM-dd}) - ITSON",
                $"<p>Adjunto se encuentra el reporte en Excel de asistencias, retardos e inasistencias del <strong>{filtro.FechaInicio:yyyy-MM-dd}</strong> al <strong>{filtro.FechaFin:yyyy-MM-dd}</strong>.</p>",
                excelBytes,
                nombreArchivo,
                cancellationToken: cancellationToken);
        }

        var registroHistorial = HistorialReporte.RegistrarReporteManual(
            TipoReporte.Asistencia,
            filtro.FechaInicio,
            filtro.FechaFin,
            nombreArchivo,
            asistencias.Count,
            adminUsuarioId,
            filtro.IdCampus,
            filtro.IdEspacio,
            filtro.IdUsuario,
            enviadoACorreo: filtro.EnviarPorCorreoA);

        await _historialReporteRepository.AddAsync(registroHistorial, cancellationToken);

        return new ArchivoReporteDto(excelBytes, nombreArchivo);
    }

    public async Task<bool> EjecutarReporteAutomaticoEventosDelDiaAsync(
        DateOnly fecha,
        CancellationToken cancellationToken = default)
    {
        var config = await _configuracionRepository.ObtenerConfiguracionActivaAsync(cancellationToken);
        if (!config.EnvioAutomaticoReportesActivo || string.IsNullOrWhiteSpace(config.CorreosDestinatariosReportes))
            return false;

        if (await _historialReporteRepository.YaSeEnvioReporteAutomaticoAsync(TipoReporte.EventosYEspacios, fecha, cancellationToken))
            return false;

        var eventosHoy = await _reservaRepository.ObtenerInstanciasPorRangoFechasAsync(
            fecha, fecha, idCampus: null, idEspacio: null, soloConfirmadas: true, cancellationToken);

        var espacios = await _espacioRepository.ObtenerConFiltrosAsync(
            idCampus: null, idTipoEspacio: null, estado: null, soloActivos: true, cancellationToken);

        var excelBytes = _excelReportGenerator.GenerarReporteEventosYEspacios(eventosHoy, espacios, fecha, fecha);
        var nombreArchivo = $"Reporte_Diario_Eventos_Espacios_{fecha:yyyy-MM-dd}.xlsx";
        var destinatarios = SepararCorreos(config.CorreosDestinatariosReportes);

        try
        {
            await _emailService.SendEmailWithAttachmentAsync(
                destinatarios,
                $"[Automático] Reporte Diario de Eventos y Espacios ({fecha:yyyy-MM-dd}) - ITSON",
                $"<p>Se adjunta el reporte automático diario de los eventos programados y el estado de los espacios culturales para el día <strong>{fecha:yyyy-MM-dd}</strong>.</p>",
                excelBytes,
                nombreArchivo,
                cancellationToken: cancellationToken);

            var historialExito = HistorialReporte.RegistrarReporteAutomatico(
                TipoReporte.EventosYEspacios,
                fecha,
                nombreArchivo,
                eventosHoy.Count,
                config.CorreosDestinatariosReportes,
                enviadoConExito: true);

            await _historialReporteRepository.AddAsync(historialExito, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            var historialError = HistorialReporte.RegistrarReporteAutomatico(
                TipoReporte.EventosYEspacios,
                fecha,
                nombreArchivo,
                eventosHoy.Count,
                config.CorreosDestinatariosReportes,
                enviadoConExito: false,
                detalleError: ex.Message);

            await _historialReporteRepository.AddAsync(historialError, cancellationToken);
            throw;
        }
    }

    public async Task<bool> EjecutarReporteAutomaticoAsistenciaDelDiaAsync(
        DateOnly fecha,
        CancellationToken cancellationToken = default)
    {
        var config = await _configuracionRepository.ObtenerConfiguracionActivaAsync(cancellationToken);
        if (!config.EnvioAutomaticoReportesActivo || string.IsNullOrWhiteSpace(config.CorreosDestinatariosReportes))
            return false;

        if (await _historialReporteRepository.YaSeEnvioReporteAutomaticoAsync(TipoReporte.Asistencia, fecha, cancellationToken))
            return false;

        // 1. Marcar como Inasistencia los eventos del día que ya concluyeron y no tuvieron registro de entrada
        await _asistenciaService.ProcesarInasistenciasVencidasAsync(fecha, new TimeOnly(23, 59, 59), cancellationToken);

        // 2. Obtener todas las asistencias del día
        var asistenciasHoy = await _asistenciaRepository.ObtenerParaReporteConFiltrosAsync(
            fecha, fecha, idCampus: null, idEspacio: null, idUsuario: null, estado: null, cancellationToken);

        var excelBytes = _excelReportGenerator.GenerarReporteAsistencia(asistenciasHoy, fecha, fecha);
        var nombreArchivo = $"Reporte_Diario_Asistencia_{fecha:yyyy-MM-dd}.xlsx";
        var destinatarios = SepararCorreos(config.CorreosDestinatariosReportes);

        try
        {
            await _emailService.SendEmailWithAttachmentAsync(
                destinatarios,
                $"[Automático] Reporte Diario de Asistencia ({fecha:yyyy-MM-dd}) - ITSON",
                $"<p>Se adjunta el corte diario automático de asistencias, retardos e inasistencias correspondientes al día <strong>{fecha:yyyy-MM-dd}</strong>.</p>",
                excelBytes,
                nombreArchivo,
                cancellationToken: cancellationToken);

            var historialExito = HistorialReporte.RegistrarReporteAutomatico(
                TipoReporte.Asistencia,
                fecha,
                nombreArchivo,
                asistenciasHoy.Count,
                config.CorreosDestinatariosReportes,
                enviadoConExito: true);

            await _historialReporteRepository.AddAsync(historialExito, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            var historialError = HistorialReporte.RegistrarReporteAutomatico(
                TipoReporte.Asistencia,
                fecha,
                nombreArchivo,
                asistenciasHoy.Count,
                config.CorreosDestinatariosReportes,
                enviadoConExito: false,
                detalleError: ex.Message);

            await _historialReporteRepository.AddAsync(historialError, cancellationToken);
            throw;
        }
    }

    private static IReadOnlyList<string> SepararCorreos(string correos)
    {
        return correos
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .ToList();
    }
}
