using SAAIC.Domain.Common;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Representa un espacio físico reservable (Aula o Foro) dentro de un Campus.
/// </summary>
public class Espacio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public int IdTipoEspacio { get; set; }
    public int IdCampus { get; set; }
    public TimeOnly HoraApertura { get; set; }
    public TimeOnly HoraCierre { get; set; }
    public EstadoEspacio Estado { get; set; } = EstadoEspacio.Disponible;
    public int CapacidadMaxima { get; set; }

    /// <summary>
    /// Token de concurrencia optimista (RowVersion en SQL Server) para prevenir condiciones de carrera.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navegación
    public TipoEspacio? TipoEspacio { get; set; }
    public Campus? Campus { get; set; }
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    // ================= REGLAS DE DOMINIO =================

    /// <summary>
    /// Valida todas las reglas intrínsecas del espacio antes de permitir una reservación.
    /// </summary>
    public void ValidarReglasDeEspacio(int maximoAsistentes, TimeOnly horaInicio, TimeOnly horaFin)
    {
        if (!Active || Estado == EstadoEspacio.Mantenimiento)
        {
            throw new DomainException($"El espacio '{Nombre}' no está disponible para reservas porque se encuentra en mantenimiento o inactivo.");
        }

        ValidarCapacidad(maximoAsistentes);
        ValidarHorarioOperativo(horaInicio, horaFin);
    }

    /// <summary>
    /// Regla de capacidad estricta: Los asistentes no pueden superar el aforo máximo del espacio.
    /// </summary>
    public void ValidarCapacidad(int maximoAsistentes)
    {
        if (maximoAsistentes <= 0)
        {
            throw new DomainException("El número de asistentes debe ser mayor a cero.");
        }

        if (maximoAsistentes > CapacidadMaxima)
        {
            throw new DomainException(
                $"El número de asistentes ({maximoAsistentes}) supera la capacidad máxima permitida para el espacio '{Nombre}' ({CapacidadMaxima} personas).");
        }
    }

    /// <summary>
    /// Valida que el rango de horas solicitado esté dentro de la ventana operativa (HoraApertura - HoraCierre).
    /// </summary>
    public void ValidarHorarioOperativo(TimeOnly horaInicio, TimeOnly horaFin)
    {
        if (horaInicio >= horaFin)
        {
            throw new DomainException("La hora de inicio debe ser estrictamente menor a la hora de finalización.");
        }

        if (horaInicio < HoraApertura || horaFin > HoraCierre)
        {
            throw new DomainException(
                $"El horario solicitado ({horaInicio:HH:mm} - {horaFin:HH:mm}) está fuera del horario operativo del espacio '{Nombre}' ({HoraApertura:HH:mm} - {HoraCierre:HH:mm}).");
        }
    }
}
