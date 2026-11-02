namespace SAAIC.Domain.Enums;

/// <summary>
/// Estado del envío por correo para un reporte generado.
/// </summary>
public enum EstadoEnvioReporte
{
    /// <summary>
    /// Solo fue generado/exportado en Excel para descarga directa (sin envío por correo).
    /// </summary>
    SoloExportado = 1,

    /// <summary>
    /// El archivo Excel fue generado y enviado por correo exitosamente.
    /// </summary>
    EnviadoPorCorreo = 2,

    /// <summary>
    /// El archivo se generó, pero ocurrió un error al enviarlo por correo.
    /// </summary>
    ErrorEnEnvio = 3
}
