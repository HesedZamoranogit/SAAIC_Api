using SAAIC.Domain.Entities;

namespace SAAIC.Application.Interfaces.Services;

/// <summary>
/// Generador de archivos Excel (.xlsx) para los dos reportes del sistema:
/// 1. Reporte de Eventos y Estado de Espacios.
/// 2. Reporte de Asistencia (Presentes, Retardos e Inasistencias).
/// </summary>
public interface IExcelReportGenerator
{
    /// <summary>
    /// Genera el archivo Excel (.xlsx) del Reporte 1: Eventos próximos y estado de los espacios (Disponibles, Ocupados, Mantenimiento).
    /// </summary>
    byte[] GenerarReporteEventosYEspacios(
        IEnumerable<Reserva> instanciasEventos,
        IEnumerable<Espacio> espacios,
        DateOnly fechaInicio,
        DateOnly fechaFin);

    /// <summary>
    /// Genera el archivo Excel (.xlsx) del Reporte 2: Asistencia de los usuarios con respecto a sus eventos.
    /// </summary>
    byte[] GenerarReporteAsistencia(
        IEnumerable<Asistencia> asistencias,
        DateOnly fechaInicio,
        DateOnly fechaFin);
}
