using SAAIC.Application.DTOs.Reportes;

namespace SAAIC.Application.Interfaces.Services;

public interface IReporteService
{
    /// <summary>
    /// Genera al momento el Reporte 1 en Excel (Eventos y Estado de Espacios) con filtros personalizados de Administrador.
    /// No afecta el corte automático diario.
    /// </summary>
    Task<ArchivoReporteDto> GenerarReporteEventosManualAsync(
        FiltroReporteEventosRequestDto filtro,
        string adminUsuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera al momento el Reporte 2 en Excel (Asistencias, Retardos e Inasistencias) con filtros personalizados de Administrador.
    /// No afecta el corte automático diario.
    /// </summary>
    Task<ArchivoReporteDto> GenerarReporteAsistenciaManualAsync(
        FiltroReporteAsistenciaRequestDto filtro,
        string adminUsuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta y envía por correo (Gmail API) el Reporte 1 automático diario de eventos próximos y espacios.
    /// </summary>
    Task<bool> EjecutarReporteAutomaticoEventosDelDiaAsync(
        DateOnly fecha,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Procesa las inasistencias del día, genera y envía por correo (Gmail API) el Reporte 2 automático diario de asistencia.
    /// </summary>
    Task<bool> EjecutarReporteAutomaticoAsistenciaDelDiaAsync(
        DateOnly fecha,
        CancellationToken cancellationToken = default);
}
