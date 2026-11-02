using SAAIC.Domain.Common;
using SAAIC.Domain.Enums;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Bitácora de reportes Excel generados (tanto automáticos diarios enviados por correo como manuales con filtros).
/// Permite garantizar que el corte automático diario no se duplique y auditar envíos de correo.
/// </summary>
public class HistorialReporte : BaseEntity
{
    public TipoReporte TipoReporte { get; set; }
    public OrigenReporte Origen { get; set; }

    /// <summary>
    /// En reportes automáticos diarios: la fecha exacta del día reportado.
    /// En reportes manuales: fecha de inicio del filtro aplicado.
    /// </summary>
    public DateOnly FechaInicioReporte { get; set; }

    /// <summary>
    /// En reportes automáticos diarios: igual a FechaInicioReporte.
    /// En reportes manuales: fecha de fin del filtro aplicado.
    /// </summary>
    public DateOnly FechaFinReporte { get; set; }

    /// <summary>
    /// Filtro opcional de Campus cuando fue generado manualmente por un Administrador.
    /// </summary>
    public int? IdCampusFiltro { get; set; }

    /// <summary>
    /// Filtro opcional de Espacio cuando fue generado manualmente por un Administrador.
    /// </summary>
    public int? IdEspacioFiltro { get; set; }

    /// <summary>
    /// Filtro opcional de Usuario/Profesor cuando fue generado manualmente por un Administrador.
    /// </summary>
    public string? IdUsuarioFiltro { get; set; }

    public string NombreArchivoExcel { get; set; } = string.Empty;
    public int TotalRegistrosIncluidos { get; set; }

    public EstadoEnvioReporte EstadoEnvio { get; set; }
    public string? DestinatariosCorreo { get; set; }
    public string? DetalleErrorEnvio { get; set; }

    public static HistorialReporte RegistrarReporteAutomatico(
        TipoReporte tipo,
        DateOnly fechaDia,
        string nombreArchivoExcel,
        int totalRegistros,
        string destinatariosCorreo,
        bool enviadoConExito,
        string? detalleError = null)
    {
        return new HistorialReporte
        {
            TipoReporte = tipo,
            Origen = OrigenReporte.AutomaticoDiario,
            FechaInicioReporte = fechaDia,
            FechaFinReporte = fechaDia,
            NombreArchivoExcel = nombreArchivoExcel,
            TotalRegistrosIncluidos = totalRegistros,
            DestinatariosCorreo = destinatariosCorreo,
            EstadoEnvio = enviadoConExito ? EstadoEnvioReporte.EnviadoPorCorreo : EstadoEnvioReporte.ErrorEnEnvio,
            DetalleErrorEnvio = detalleError,
            CreatedBy = "SYSTEM"
        };
    }

    public static HistorialReporte RegistrarReporteManual(
        TipoReporte tipo,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        string nombreArchivoExcel,
        int totalRegistros,
        string adminUsuarioId,
        int? idCampus = null,
        int? idEspacio = null,
        string? idUsuarioFiltro = null,
        string? enviadoACorreo = null)
    {
        return new HistorialReporte
        {
            TipoReporte = tipo,
            Origen = OrigenReporte.ManualConFiltros,
            FechaInicioReporte = fechaInicio,
            FechaFinReporte = fechaFin,
            IdCampusFiltro = idCampus,
            IdEspacioFiltro = idEspacio,
            IdUsuarioFiltro = idUsuarioFiltro,
            NombreArchivoExcel = nombreArchivoExcel,
            TotalRegistrosIncluidos = totalRegistros,
            DestinatariosCorreo = enviadoACorreo,
            EstadoEnvio = string.IsNullOrWhiteSpace(enviadoACorreo)
                ? EstadoEnvioReporte.SoloExportado
                : EstadoEnvioReporte.EnviadoPorCorreo,
            CreatedBy = adminUsuarioId
        };
    }
}
