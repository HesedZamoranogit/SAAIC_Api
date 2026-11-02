using Microsoft.AspNetCore.Identity;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Usuario del sistema (Profesor o Administrador) integrado con ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// ID institucional obligatorio de ITSON (ej. ID de empleado/docente/alumno).
    /// Utilizado como identificador principal para el inicio de sesión.
    /// </summary>
    public string IdInstitucional { get; set; } = string.Empty;

    public string? Departamento { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navegación
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
}
