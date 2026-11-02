namespace SAAIC.Domain.Enums;

/// <summary>
/// Indica si el reporte fue ejecutado por el proceso automático diario o generado manualmente por un Administrador.
/// </summary>
public enum OrigenReporte
{
    /// <summary>
    /// Generado automáticamente por el sistema (corte diario) y enviado por correo electrónico.
    /// </summary>
    AutomaticoDiario = 1,

    /// <summary>
    /// Generado al momento por un Administrador aplicando filtros personalizados (no afecta el reporte automático).
    /// </summary>
    ManualConFiltros = 2
}
