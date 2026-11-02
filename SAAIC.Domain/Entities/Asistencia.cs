using SAAIC.Domain.Common;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Registro de asistencia (entrada, salida, retardos e inasistencias) asociado a una instancia diaria de reservación.
/// En reservas de varios días (Jornada de 1 a 7 días o Semestral), existe un registro de Asistencia por cada día del evento.
/// </summary>
public class Asistencia : BaseEntity
{
    public int IdReserva { get; set; }
    public string IdUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Fecha específica del día del evento al que corresponde esta asistencia.
    /// </summary>
    public DateOnly FechaEvento { get; set; }

    public TimeOnly HoraProgramadaInicio { get; set; }
    public TimeOnly HoraProgramadaFin { get; set; }

    /// <summary>
    /// Fecha y hora exacta en que el responsable marcó su entrada (Check-In).
    /// </summary>
    public DateTime? HoraEntrada { get; set; }

    /// <summary>
    /// Fecha y hora exacta en que el responsable marcó su salida (Check-Out).
    /// </summary>
    public DateTime? HoraSalida { get; set; }

    public EstadoAsistencia Estado { get; set; } = EstadoAsistencia.Pendiente;

    /// <summary>
    /// Cantidad de minutos de retraso con respecto a la HoraProgramadaInicio (0 si llegó a tiempo).
    /// </summary>
    public int MinutosRetardo { get; set; }

    /// <summary>
    /// Minutos de tolerancia vigentes en el sistema al momento de evaluar la entrada (auditoría histórica).
    /// </summary>
    public int MinutosToleranciaAplicados { get; set; }

    public string? Observaciones { get; set; }

    // ================= PROPIEDADES CALCULADAS =================

    public bool MarcoEntrada => HoraEntrada.HasValue;
    public bool MarcoSalida => HoraSalida.HasValue;
    public bool AsistenciaCompleta => HoraEntrada.HasValue && HoraSalida.HasValue;

    // ================= NAVEGACIÓN =================

    public Reserva? Reserva { get; set; }
    public ApplicationUser? Usuario { get; set; }

    // ================= FÁBRICA Y REGLAS DE DOMINIO =================

    /// <summary>
    /// Crea el registro de asistencia inicial (Pendiente) para una instancia diaria de reservación.
    /// </summary>
    public static Asistencia CrearParaInstancia(Reserva instanciaReserva)
    {
        ArgumentNullException.ThrowIfNull(instanciaReserva);

        if (instanciaReserva.EsReservaPadre)
        {
            throw new DomainException("No se puede crear un registro de asistencia sobre una cabecera Padre; la asistencia se registra por cada instancia diaria del evento.");
        }

        return new Asistencia
        {
            Reserva = instanciaReserva,
            IdReserva = instanciaReserva.Id,
            IdUsuario = instanciaReserva.IdUsuario,
            FechaEvento = instanciaReserva.FechaInicio,
            HoraProgramadaInicio = instanciaReserva.HoraInicio,
            HoraProgramadaFin = instanciaReserva.HoraFin,
            Estado = EstadoAsistencia.Pendiente,
            CreatedBy = instanciaReserva.IdUsuario
        };
    }

    /// <summary>
    /// Registra la entrada (Check-In) del responsable en el día del evento, evaluando si está en tiempo o con retardo
    /// según los minutos de tolerancia configurados en el sistema.
    /// </summary>
    /// <param name="fechaHoraRegistro">Fecha y hora actual del servidor (local de la sede/campus).</param>
    /// <param name="minutosToleranciaRetardo">Minutos configurados antes de considerar retardo.</param>
    /// <param name="minutosAnticipacionPermitidos">Minutos antes de la hora de inicio en que ya se permite marcar entrada.</param>
    public void RegistrarEntrada(
        DateTime fechaHoraRegistro,
        int minutosToleranciaRetardo,
        int minutosAnticipacionPermitidos = 30)
    {
        if (minutosToleranciaRetardo < 0)
            throw new DomainException("El tiempo de tolerancia para retardo no puede ser negativo.");

        if (HoraEntrada.HasValue)
            throw new DomainException("Ya se ha registrado la entrada para este día del evento.");

        if (Estado == EstadoAsistencia.Inasistencia)
            throw new DomainException("No se puede registrar entrada porque el evento ya fue cerrado como inasistencia.");

        var fechaActual = DateOnly.FromDateTime(fechaHoraRegistro);
        if (fechaActual != FechaEvento)
        {
            throw new DomainException($"Solo puedes registrar la entrada en la fecha programada del evento ({FechaEvento:yyyy-MM-dd}).");
        }

        var inicioProgramadoDateTime = FechaEvento.ToDateTime(HoraProgramadaInicio);
        var finProgramadoDateTime = FechaEvento.ToDateTime(HoraProgramadaFin);
        var aperturaCheckInDateTime = inicioProgramadoDateTime.AddMinutes(-minutosAnticipacionPermitidos);

        if (fechaHoraRegistro < aperturaCheckInDateTime)
        {
            throw new DomainException(
                $"Aún es demasiado temprano para registrar la entrada. Puedes marcar desde {minutosAnticipacionPermitidos} minutos antes del inicio ({aperturaCheckInDateTime:HH:mm}).");
        }

        if (fechaHoraRegistro > finProgramadoDateTime)
        {
            throw new DomainException(
                $"El horario del evento ({HoraProgramadaInicio:HH:mm} - {HoraProgramadaFin:HH:mm}) ya concluyó; no es posible registrar la entrada.");
        }

        HoraEntrada = fechaHoraRegistro;
        MinutosToleranciaAplicados = minutosToleranciaRetardo;

        var limiteToleranciaDateTime = inicioProgramadoDateTime.AddMinutes(minutosToleranciaRetardo);
        var diferenciaMinutos = (int)Math.Floor((fechaHoraRegistro - inicioProgramadoDateTime).TotalMinutes);

        MinutosRetardo = Math.Max(0, diferenciaMinutos);

        if (fechaHoraRegistro <= limiteToleranciaDateTime)
        {
            Estado = EstadoAsistencia.Presente;
        }
        else
        {
            Estado = EstadoAsistencia.Retardo;
            Observaciones = $"Entrada con retardo de {MinutosRetardo} min (tolerancia configurada: {minutosToleranciaRetardo} min).";
        }

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = IdUsuario;
    }

    /// <summary>
    /// Registra la salida (Check-Out) del responsable en el día del evento.
    /// </summary>
    public void RegistrarSalida(DateTime fechaHoraRegistro)
    {
        if (!HoraEntrada.HasValue)
        {
            throw new DomainException("No puedes registrar la salida sin haber registrado primero la entrada al evento.");
        }

        if (HoraSalida.HasValue)
        {
            throw new DomainException("Ya se ha registrado la salida para este día del evento.");
        }

        if (fechaHoraRegistro < HoraEntrada.Value)
        {
            throw new DomainException("La hora de salida no puede ser anterior a la hora de entrada registrada.");
        }

        HoraSalida = fechaHoraRegistro;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = IdUsuario;
    }

    /// <summary>
    /// Marca automáticamente como Inasistencia cuando el evento concluyó y el responsable nunca registró entrada.
    /// </summary>
    public void MarcarInasistencia(string? motivo = null, string ejecutadoPor = "SYSTEM")
    {
        if (HoraEntrada.HasValue)
        {
            throw new DomainException("No se puede marcar inasistencia porque el usuario sí registró su entrada.");
        }

        Estado = EstadoAsistencia.Inasistencia;
        Observaciones = string.IsNullOrWhiteSpace(motivo)
            ? "Inasistencia: No se registró entrada durante el periodo del evento."
            : motivo.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = ejecutadoPor;
    }

    /// <summary>
    /// Permite a un Administrador justificar una inasistencia o retardo.
    /// </summary>
    public void Justificar(string adminUsuarioId, string motivoJustificacion)
    {
        if (string.IsNullOrWhiteSpace(motivoJustificacion))
        {
            throw new DomainException("Debe proporcionar un motivo para justificar la asistencia.");
        }

        Estado = EstadoAsistencia.Justificada;
        Observaciones = $"Justificada por Admin: {motivoJustificacion.Trim()}";
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = adminUsuarioId;
    }
}
