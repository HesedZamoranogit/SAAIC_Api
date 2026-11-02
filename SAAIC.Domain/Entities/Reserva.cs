using SAAIC.Domain.Common;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Entidad principal de reservación. Soporta:
/// 1. PorPeriodo (bloque de horas en 1 día).
/// 2. PorJornada (todo el horario del espacio, de 1 a 7 días dentro de un rango máximo de 7 días).
/// 3. RecurrenteSemestral (repetición semanal en días seleccionados durante el semestre).
/// Utiliza el patrón Padre-Hija (Self-Referencing) para reservas multi-día, generando una Asistencia por cada día de evento.
/// </summary>
public class Reserva : BaseEntity
{
    public const int MaximoDiasRangoJornada = 7;

    public int IdEspacio { get; set; }
    public int IdCampus { get; set; }
    public string IdUsuario { get; set; } = string.Empty;

    public string ResponsableEvento { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int MaximoAsistentes { get; set; }
    public string? NotaSeguridad { get; set; }

    public TipoReserva TipoReserva { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Confirmada;

    /// <summary>
    /// En instancias de un día (EsReservaPadre = false): es la fecha exacta del evento.
    /// En cabeceras Padre (Jornada multi-día o Semestral): es la primera fecha del rango.
    /// </summary>
    public DateOnly FechaInicio { get; set; }

    /// <summary>
    /// En instancias de un día (EsReservaPadre = false): es igual a FechaInicio.
    /// En cabeceras Padre (Jornada multi-día o Semestral): es la última fecha del rango.
    /// </summary>
    public DateOnly FechaFin { get; set; }

    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    /// <summary>
    /// Días de la semana seleccionados (ej. Lunes | Miercoles | Viernes).
    /// </summary>
    public DiasSemana DiasRecurrencia { get; set; } = DiasSemana.Ninguno;

    // ================= JERARQUÍA PADRE-HIJA =================

    /// <summary>
    /// Si tiene valor, esta reserva es una instancia diaria (Hija) perteneciente a una reserva Padre (Jornada multi-día o Semestral).
    /// </summary>
    public int? IdReservaPadre { get; set; }

    /// <summary>
    /// True si este registro es el agrupador Padre de una reserva de varios días (Jornada multi-día o Semestral).
    /// False si es una instancia diaria real que ocupa el espacio en una fecha específica y requiere marca de entrada/salida.
    /// </summary>
    public bool EsReservaPadre { get; set; }

    // ================= AUDITORÍA DE CANCELACIÓN Y CONCURRENCIA =================

    public DateTime? FechaCancelacion { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string? CanceladaPorUsuarioId { get; set; }

    /// <summary>
    /// Token de concurrencia optimista (RowVersion en SQL Server).
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // ================= PROPIEDADES DE NAVEGACIÓN =================

    public Espacio? Espacio { get; set; }
    public Campus? Campus { get; set; }
    public ApplicationUser? Usuario { get; set; }
    public Reserva? ReservaPadre { get; set; }
    public ICollection<Reserva> ReservasHijas { get; set; } = new List<Reserva>();

    /// <summary>
    /// Registro de asistencia (entrada, salida, retardo o inasistencia) para esta instancia diaria del evento.
    /// </summary>
    public Asistencia? Asistencia { get; set; }

    // ================= FÁBRICAS Y REGLAS DE DOMINIO =================

    /// <summary>
    /// Crea una reserva por periodo de horas dentro de un día específico, incluyendo su registro de asistencia.
    /// </summary>
    public static Reserva CrearPorPeriodo(
        Espacio espacio,
        string idUsuario,
        string responsableEvento,
        string descripcion,
        int maximoAsistentes,
        DateOnly fecha,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        string? notaSeguridad = null)
    {
        ArgumentNullException.ThrowIfNull(espacio);
        ValidarDatosBasicos(idUsuario, responsableEvento, descripcion);
        espacio.ValidarReglasDeEspacio(maximoAsistentes, horaInicio, horaFin);

        var reserva = new Reserva
        {
            IdEspacio = espacio.Id,
            IdCampus = espacio.IdCampus,
            IdUsuario = idUsuario,
            ResponsableEvento = responsableEvento.Trim(),
            Descripcion = descripcion.Trim(),
            MaximoAsistentes = maximoAsistentes,
            NotaSeguridad = string.IsNullOrWhiteSpace(notaSeguridad) ? null : notaSeguridad.Trim(),
            TipoReserva = TipoReserva.PorPeriodo,
            Estado = EstadoReserva.Confirmada,
            FechaInicio = fecha,
            FechaFin = fecha,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            DiasRecurrencia = fecha.DayOfWeek.ToDiasSemanaFlag(),
            EsReservaPadre = false,
            CreatedBy = idUsuario
        };

        reserva.Asistencia = Asistencia.CrearParaInstancia(reserva);
        return reserva;
    }

    /// <summary>
    /// Crea una reserva por jornada completa para un rango continuo de fechas (de 1 a 7 días máximo).
    /// </summary>
    public static Reserva CrearPorJornada(
        Espacio espacio,
        string idUsuario,
        string responsableEvento,
        string descripcion,
        int maximoAsistentes,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        string? notaSeguridad = null)
    {
        if (fechaFin < fechaInicio)
        {
            throw new DomainException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        int totalDiasRango = (fechaFin.DayNumber - fechaInicio.DayNumber) + 1;
        if (totalDiasRango > MaximoDiasRangoJornada)
        {
            var fechaLimite = fechaInicio.AddDays(MaximoDiasRangoJornada - 1);
            throw new DomainException(
                $"Las reservaciones por jornada tienen un rango máximo de {MaximoDiasRangoJornada} días. Si el primer día es {fechaInicio:yyyy-MM-dd}, la fecha máxima permitida es {fechaLimite:yyyy-MM-dd}.");
        }

        var fechas = new List<DateOnly>(totalDiasRango);
        for (var fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
        {
            fechas.Add(fecha);
        }

        return CrearPorJornada(espacio, idUsuario, responsableEvento, descripcion, maximoAsistentes, fechas, notaSeguridad);
    }

    /// <summary>
    /// Crea una reserva por jornada completa seleccionando 1 o varios días específicos dentro de una ventana máxima de 7 días
    /// contados desde el primer día seleccionado (ej. si inicia el 1 de enero, no se puede seleccionar más allá del 7 de enero).
    /// Genera una instancia diaria con su propio registro de entrada/salida (Asistencia) por cada día seleccionado.
    /// </summary>
    public static Reserva CrearPorJornada(
        Espacio espacio,
        string idUsuario,
        string responsableEvento,
        string descripcion,
        int maximoAsistentes,
        IEnumerable<DateOnly> fechasSeleccionadas,
        string? notaSeguridad = null)
    {
        ArgumentNullException.ThrowIfNull(espacio);
        ArgumentNullException.ThrowIfNull(fechasSeleccionadas);
        ValidarDatosBasicos(idUsuario, responsableEvento, descripcion);

        var fechasOrdenadas = fechasSeleccionadas
            .Distinct()
            .OrderBy(f => f)
            .ToList();

        if (fechasOrdenadas.Count == 0)
        {
            throw new DomainException("Debe seleccionar al menos un día para la reservación por jornada.");
        }

        var primeraFecha = fechasOrdenadas.First();
        var ultimaFecha = fechasOrdenadas.Last();
        int diasDiferenciaInclusiva = (ultimaFecha.DayNumber - primeraFecha.DayNumber) + 1;

        if (diasDiferenciaInclusiva > MaximoDiasRangoJornada)
        {
            var fechaLimite = primeraFecha.AddDays(MaximoDiasRangoJornada - 1);
            throw new DomainException(
                $"Las reservaciones por jornada tienen un rango máximo de {MaximoDiasRangoJornada} días. Si el primer día seleccionado es {primeraFecha:yyyy-MM-dd}, no puedes seleccionar fechas posteriores al {fechaLimite:yyyy-MM-dd}.");
        }

        espacio.ValidarReglasDeEspacio(maximoAsistentes, espacio.HoraApertura, espacio.HoraCierre);

        var notaLimpia = string.IsNullOrWhiteSpace(notaSeguridad) ? null : notaSeguridad.Trim();
        var responsableLimpio = responsableEvento.Trim();
        var descripcionLimpia = descripcion.Trim();

        // Si solo seleccionó 1 día, se crea directamente la instancia diaria con su asistencia
        if (fechasOrdenadas.Count == 1)
        {
            var unicaFecha = fechasOrdenadas[0];
            var reservaUnica = new Reserva
            {
                IdEspacio = espacio.Id,
                IdCampus = espacio.IdCampus,
                IdUsuario = idUsuario,
                ResponsableEvento = responsableLimpio,
                Descripcion = descripcionLimpia,
                MaximoAsistentes = maximoAsistentes,
                NotaSeguridad = notaLimpia,
                TipoReserva = TipoReserva.PorJornada,
                Estado = EstadoReserva.Confirmada,
                FechaInicio = unicaFecha,
                FechaFin = unicaFecha,
                HoraInicio = espacio.HoraApertura,
                HoraFin = espacio.HoraCierre,
                DiasRecurrencia = unicaFecha.DayOfWeek.ToDiasSemanaFlag(),
                EsReservaPadre = false,
                CreatedBy = idUsuario
            };

            reservaUnica.Asistencia = Asistencia.CrearParaInstancia(reservaUnica);
            return reservaUnica;
        }

        // Si seleccionó varios días (2 a 7 días), se crea la cabecera Padre y una instancia Hija con Asistencia por cada día
        DiasSemana mascaraDias = DiasSemana.Ninguno;
        foreach (var f in fechasOrdenadas)
        {
            mascaraDias |= f.DayOfWeek.ToDiasSemanaFlag();
        }

        var reservaPadre = new Reserva
        {
            IdEspacio = espacio.Id,
            IdCampus = espacio.IdCampus,
            IdUsuario = idUsuario,
            ResponsableEvento = responsableLimpio,
            Descripcion = descripcionLimpia,
            MaximoAsistentes = maximoAsistentes,
            NotaSeguridad = notaLimpia,
            TipoReserva = TipoReserva.PorJornada,
            Estado = EstadoReserva.Confirmada,
            FechaInicio = primeraFecha,
            FechaFin = ultimaFecha,
            HoraInicio = espacio.HoraApertura,
            HoraFin = espacio.HoraCierre,
            DiasRecurrencia = mascaraDias,
            EsReservaPadre = true,
            CreatedBy = idUsuario
        };

        foreach (var fechaDia in fechasOrdenadas)
        {
            var instanciaHija = new Reserva
            {
                IdEspacio = espacio.Id,
                IdCampus = espacio.IdCampus,
                IdUsuario = idUsuario,
                ResponsableEvento = responsableLimpio,
                Descripcion = descripcionLimpia,
                MaximoAsistentes = maximoAsistentes,
                NotaSeguridad = notaLimpia,
                TipoReserva = TipoReserva.PorJornada,
                Estado = EstadoReserva.Confirmada,
                FechaInicio = fechaDia,
                FechaFin = fechaDia,
                HoraInicio = espacio.HoraApertura,
                HoraFin = espacio.HoraCierre,
                DiasRecurrencia = fechaDia.DayOfWeek.ToDiasSemanaFlag(),
                EsReservaPadre = false,
                ReservaPadre = reservaPadre,
                CreatedBy = idUsuario
            };

            instanciaHija.Asistencia = Asistencia.CrearParaInstancia(instanciaHija);
            reservaPadre.ReservasHijas.Add(instanciaHija);
        }

        return reservaPadre;
    }

    /// <summary>
    /// Crea una reserva recurrente semestral (Padre) junto con todas sus instancias diarias (Hijas)
    /// y los registros de asistencia diarios correspondientes.
    /// </summary>
    public static Reserva CrearRecurrenteSemestral(
        Espacio espacio,
        string idUsuario,
        string responsableEvento,
        string descripcion,
        int maximoAsistentes,
        DateOnly fechaInicioSemestre,
        DateOnly fechaFinSemestre,
        TimeOnly horaInicio,
        TimeOnly horaFin,
        DiasSemana diasSeleccionados,
        string? notaSeguridad = null)
    {
        ArgumentNullException.ThrowIfNull(espacio);
        ValidarDatosBasicos(idUsuario, responsableEvento, descripcion);

        if (fechaFinSemestre < fechaInicioSemestre)
        {
            throw new DomainException("La fecha de fin del periodo semestral no puede ser anterior a la fecha de inicio.");
        }

        if (diasSeleccionados == DiasSemana.Ninguno)
        {
            throw new DomainException("Debe seleccionar al menos un día de la semana para una reservación recurrente semestral.");
        }

        espacio.ValidarReglasDeEspacio(maximoAsistentes, horaInicio, horaFin);

        var reservaPadre = new Reserva
        {
            IdEspacio = espacio.Id,
            IdCampus = espacio.IdCampus,
            IdUsuario = idUsuario,
            ResponsableEvento = responsableEvento.Trim(),
            Descripcion = descripcion.Trim(),
            MaximoAsistentes = maximoAsistentes,
            NotaSeguridad = string.IsNullOrWhiteSpace(notaSeguridad) ? null : notaSeguridad.Trim(),
            TipoReserva = TipoReserva.RecurrenteSemestral,
            Estado = EstadoReserva.Confirmada,
            FechaInicio = fechaInicioSemestre,
            FechaFin = fechaFinSemestre,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            DiasRecurrencia = diasSeleccionados,
            EsReservaPadre = true,
            CreatedBy = idUsuario
        };

        for (var fechaActual = fechaInicioSemestre; fechaActual <= fechaFinSemestre; fechaActual = fechaActual.AddDays(1))
        {
            if (diasSeleccionados.IncluyeFecha(fechaActual))
            {
                var instanciaHija = new Reserva
                {
                    IdEspacio = espacio.Id,
                    IdCampus = espacio.IdCampus,
                    IdUsuario = idUsuario,
                    ResponsableEvento = reservaPadre.ResponsableEvento,
                    Descripcion = reservaPadre.Descripcion,
                    MaximoAsistentes = maximoAsistentes,
                    NotaSeguridad = reservaPadre.NotaSeguridad,
                    TipoReserva = TipoReserva.RecurrenteSemestral,
                    Estado = EstadoReserva.Confirmada,
                    FechaInicio = fechaActual,
                    FechaFin = fechaActual,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                    DiasRecurrencia = fechaActual.DayOfWeek.ToDiasSemanaFlag(),
                    EsReservaPadre = false,
                    ReservaPadre = reservaPadre,
                    CreatedBy = idUsuario
                };

                instanciaHija.Asistencia = Asistencia.CrearParaInstancia(instanciaHija);
                reservaPadre.ReservasHijas.Add(instanciaHija);
            }
        }

        if (reservaPadre.ReservasHijas.Count == 0)
        {
            throw new DomainException(
                "El rango de fechas proporcionado no contiene ningún día que coincida con los días de la semana seleccionados.");
        }

        return reservaPadre;
    }

    /// <summary>
    /// Determina si esta instancia diaria de reserva colisiona en fecha y horario con el bloque solicitado.
    /// Regla matemática de traslape de intervalos: (StartA &lt; EndB) y (EndA &gt; StartB).
    /// </summary>
    public bool SeSolapaCon(DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin)
    {
        if (EsReservaPadre || Estado != EstadoReserva.Confirmada)
            return false;

        if (FechaInicio != fecha)
            return false;

        return HoraInicio < horaFin && HoraFin > horaInicio;
    }

    /// <summary>
    /// Cancela la reservación. Si es una instancia hija o reserva de un día, solo cancela ese día y desactiva su asistencia.
    /// Si es la cabecera Padre (Jornada multi-día o Semestral), cancela toda la serie y sus instancias hijas activas.
    /// </summary>
    public void Cancelar(string usuarioEjecutorId, bool esAdministrador, string? motivo = null)
    {
        if (Estado == EstadoReserva.Cancelada)
        {
            throw new DomainException("La reservación ya se encuentra cancelada.");
        }

        if (!esAdministrador && IdUsuario != usuarioEjecutorId)
        {
            throw new DomainException("No tienes permisos para cancelar una reservación que pertenece a otro profesor.");
        }

        var ahora = DateTime.UtcNow;
        Estado = EstadoReserva.Cancelada;
        FechaCancelacion = ahora;
        CanceladaPorUsuarioId = usuarioEjecutorId;
        MotivoCancelacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        UpdatedAt = ahora;
        UpdatedBy = usuarioEjecutorId;

        if (Asistencia != null)
        {
            Asistencia.Active = false;
            Asistencia.UpdatedAt = ahora;
            Asistencia.UpdatedBy = usuarioEjecutorId;
        }

        // Si se cancela la cabecera Padre, se cancelan todas sus instancias hijas confirmadas y sus asistencias
        if (EsReservaPadre && ReservasHijas.Count > 0)
        {
            foreach (var hija in ReservasHijas.Where(h => h.Estado == EstadoReserva.Confirmada))
            {
                hija.Estado = EstadoReserva.Cancelada;
                hija.FechaCancelacion = ahora;
                hija.CanceladaPorUsuarioId = usuarioEjecutorId;
                hija.MotivoCancelacion = MotivoCancelacion ?? "Cancelación de reservación completa.";
                hija.UpdatedAt = ahora;
                hija.UpdatedBy = usuarioEjecutorId;

                if (hija.Asistencia != null)
                {
                    hija.Asistencia.Active = false;
                    hija.Asistencia.UpdatedAt = ahora;
                    hija.Asistencia.UpdatedBy = usuarioEjecutorId;
                }
            }
        }
    }

    /// <summary>
    /// Permite a un Administrador reasignar la reservación (y su control de asistencia pendiente) a otro profesor/responsable.
    /// </summary>
    public void Reasignar(string nuevoIdUsuario, string nuevoResponsableEvento, bool ejecutadoPorAdministrador, string? adminEjecutorId = null)
    {
        if (!ejecutadoPorAdministrador)
        {
            throw new DomainException("Solo un Administrador puede reasignar reservaciones.");
        }

        if (string.IsNullOrWhiteSpace(nuevoIdUsuario))
            throw new DomainException("El usuario destino de la reasignación es obligatorio.");

        if (string.IsNullOrWhiteSpace(nuevoResponsableEvento))
            throw new DomainException("El nombre del nuevo responsable del evento es obligatorio.");

        var ahora = DateTime.UtcNow;
        IdUsuario = nuevoIdUsuario;
        ResponsableEvento = nuevoResponsableEvento.Trim();
        UpdatedAt = ahora;
        UpdatedBy = adminEjecutorId;

        if (Asistencia != null && Asistencia.Estado == EstadoAsistencia.Pendiente)
        {
            Asistencia.IdUsuario = nuevoIdUsuario;
            Asistencia.UpdatedAt = ahora;
            Asistencia.UpdatedBy = adminEjecutorId;
        }

        if (EsReservaPadre && ReservasHijas.Count > 0)
        {
            foreach (var hija in ReservasHijas.Where(h => h.Estado == EstadoReserva.Confirmada))
            {
                hija.IdUsuario = nuevoIdUsuario;
                hija.ResponsableEvento = ResponsableEvento;
                hija.UpdatedAt = ahora;
                hija.UpdatedBy = adminEjecutorId;

                if (hija.Asistencia != null && hija.Asistencia.Estado == EstadoAsistencia.Pendiente)
                {
                    hija.Asistencia.IdUsuario = nuevoIdUsuario;
                    hija.Asistencia.UpdatedAt = ahora;
                    hija.Asistencia.UpdatedBy = adminEjecutorId;
                }
            }
        }
    }

    private static void ValidarDatosBasicos(string idUsuario, string responsableEvento, string descripcion)
    {
        if (string.IsNullOrWhiteSpace(idUsuario))
            throw new DomainException("El usuario que realiza la reserva es obligatorio.");

        if (string.IsNullOrWhiteSpace(responsableEvento))
            throw new DomainException("El nombre del responsable del evento es obligatorio.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción del evento es obligatoria.");
    }
}
