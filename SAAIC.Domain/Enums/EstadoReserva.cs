namespace SAAIC.Domain.Enums;

/// <summary>
/// Estado actual de una reservación o instancia diaria.
/// </summary>
public enum EstadoReserva
{
    /// <summary>
    /// Reservación activa y asignada (First-Come, First-Served).
    /// </summary>
    Confirmada = 1,

    /// <summary>
    /// Reservación cancelada por el profesor propietario o por un administrador (libera el espacio).
    /// </summary>
    Cancelada = 2,

    /// <summary>
    /// Reservación cuya fecha y hora ya concluyeron.
    /// </summary>
    Finalizada = 3
}
