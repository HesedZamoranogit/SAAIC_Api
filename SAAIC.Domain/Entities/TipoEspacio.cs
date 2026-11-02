using SAAIC.Domain.Common;

namespace SAAIC.Domain.Entities;

/// <summary>
/// Clasificación del espacio cultural (de base: "Aula" y "Foro").
/// </summary>
public class TipoEspacio : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // Navegación
    public ICollection<Espacio> Espacios { get; set; } = new List<Espacio>();
}
