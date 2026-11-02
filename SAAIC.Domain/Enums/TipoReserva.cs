namespace SAAIC.Domain.Enums;

/// <summary>
/// Modalidad de la reservación.
/// </summary>
public enum TipoReserva
{
    /// <summary>
    /// Reserva de un bloque de horas específico en un día (permite múltiples reservas en el día si no se solapan).
    /// </summary>
    PorPeriodo = 1,

    /// <summary>
    /// Reserva que abarca toda la jornada operativa del espacio (desde HoraApertura hasta HoraCierre) en un día.
    /// </summary>
    PorJornada = 2,

    /// <summary>
    /// Reserva semestral (Padre-Hija) que se repite semanalmente en los días seleccionados a la misma hora.
    /// </summary>
    RecurrenteSemestral = 3
}
