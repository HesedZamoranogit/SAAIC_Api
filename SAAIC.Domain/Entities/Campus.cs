using SAAIC.Domain.Common;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Campus o unidad académica de ITSON (ej. Náinari, Centro, Guaymas, Navojoa, Empalme).
/// </summary>
public class Campus : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Direccion { get; set; }

    // Navegación
    public ICollection<Espacio> Espacios { get; set; } = new List<Espacio>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
