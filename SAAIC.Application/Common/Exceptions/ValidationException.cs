namespace SAAIC.Application.Common.Exceptions;

/// <summary>
/// Excepcion lanzada cuando fallan las validaciones de datos de entrada.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Se presentaron uno o más errores de validación.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string errorMessage)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Se presentaron uno o más errores de validación.")
    {
        Errors = errors;
    }
}
