namespace SAAIC.Domain.Enums;

/// <summary>
/// Estado de la asistencia del responsable en una instancia diaria de reservación.
/// </summary>
public enum EstadoAsistencia
{
    /// <summary>
    /// El evento aún no inicia o está en curso sin registro de entrada.
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// El usuario registró su entrada dentro del tiempo de tolerancia permitido.
    /// </summary>
    Presente = 2,

    /// <summary>
    /// El usuario registró su entrada después del tiempo de tolerancia configurado.
    /// </summary>
    Retardo = 3,

    /// <summary>
    /// El usuario no registró entrada para el evento.
    /// </summary>
    Inasistencia = 4,

    /// <summary>
    /// Inasistencia o retardo justificado por un Administrador.
    /// </summary>
    Justificada = 5
}
