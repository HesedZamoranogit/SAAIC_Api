namespace SAAIC.Application.Common.Models;

/// <summary>
/// Envoltorio estandar para respuestas HTTP consistentes.
/// </summary>
/// <typeparam name="T">Tipo de dato de la respuesta.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> FailureResult(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? Enumerable.Empty<string>()
        };
    }
}
