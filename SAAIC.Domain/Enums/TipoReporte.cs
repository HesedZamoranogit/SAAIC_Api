namespace SAAIC.Domain.Enums;

/// <summary>
/// Tipos de reportes en Excel generados por el sistema.
/// </summary>
public enum TipoReporte
{
    /// <summary>
    /// Reporte de eventos próximos del día y estado general de los espacios (Ocupados, Disponibles, Mantenimiento).
    /// </summary>
    EventosYEspacios = 1,

    /// <summary>
    /// Reporte de asistencia, retardos e inasistencias de los usuarios respecto a sus eventos.
    /// </summary>
    Asistencia = 2
}
