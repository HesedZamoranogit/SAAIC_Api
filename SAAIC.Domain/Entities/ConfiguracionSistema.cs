using SAAIC.Domain.Common;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Parámetros globales configurables del sistema (tolerancia de retardo para asistencia y programación de reportes automáticos).
/// </summary>
public class ConfiguracionSistema : BaseEntity
{
    /// <summary>
    /// Minutos de tolerancia después de HoraInicio antes de marcar Retardo en la asistencia (por defecto 15 min).
    /// </summary>
    public int MinutosToleranciaRetardo { get; set; } = 15;

    /// <summary>
    /// Minutos previos al inicio del evento en que se habilita el botón de registro de entrada (por defecto 30 min).
    /// </summary>
    public int MinutosAnticipacionCheckIn { get; set; } = 30;

    /// <summary>
    /// Indica si el envío diario automático de reportes en Excel por correo está habilitado.
    /// </summary>
    public bool EnvioAutomaticoReportesActivo { get; set; } = true;

    /// <summary>
    /// Hora diaria programada para enviar el Reporte 1: Eventos próximos del día y estado de los espacios.
    /// </summary>
    public TimeOnly HoraEnvioReporteEventos { get; set; } = new(7, 0);

    /// <summary>
    /// Hora diaria programada para enviar el Reporte 2: Corte diario de asistencias, retardos e inasistencias.
    /// </summary>
    public TimeOnly HoraEnvioReporteAsistencia { get; set; } = new(22, 0);

    /// <summary>
    /// Lista de correos electrónicos destinatarios para los reportes automáticos diarios (separados por coma o punto y coma).
    /// </summary>
    public string CorreosDestinatariosReportes { get; set; } = string.Empty;

    public void ActualizarConfiguracionAsistencia(int minutosToleranciaRetardo, int minutosAnticipacionCheckIn, string adminId)
    {
        if (minutosToleranciaRetardo < 0 || minutosToleranciaRetardo > 120)
        {
            throw new DomainException("El tiempo de tolerancia para retardo debe estar entre 0 y 120 minutos.");
        }

        if (minutosAnticipacionCheckIn < 0 || minutosAnticipacionCheckIn > 180)
        {
            throw new DomainException("El tiempo de anticipación para Check-In debe estar entre 0 y 180 minutos.");
        }

        MinutosToleranciaRetardo = minutosToleranciaRetardo;
        MinutosAnticipacionCheckIn = minutosAnticipacionCheckIn;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = adminId;
    }

    public void ActualizarConfiguracionReportes(
        bool envioAutomaticoActivo,
        TimeOnly horaReporteEventos,
        TimeOnly horaReporteAsistencia,
        string? correosDestinatarios,
        string adminId)
    {
        EnvioAutomaticoReportesActivo = envioAutomaticoActivo;
        HoraEnvioReporteEventos = horaReporteEventos;
        HoraEnvioReporteAsistencia = horaReporteAsistencia;
        CorreosDestinatariosReportes = correosDestinatarios?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = adminId;
    }
}
