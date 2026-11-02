namespace SAAIC.Application.Common.Exceptions;

/// <summary>
/// Excepcion lanzada cuando un recurso solicitado no existe.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"La entidad \"{name}\" con identificador ({key}) no fue encontrada.")
    {
    }
}
