using System.Net;
using System.Text.Json;
using SAAIC.Application.Common.Exceptions;
using SAAIC.Application.Common.Models;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Web.Middlewares;

/// <summary>
/// Middleware global para captura y estandarización de excepciones no controladas.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ha ocurrido una excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = ApiResponse<object>.FailureResult(
                    validationEx.Message,
                    validationEx.Errors.SelectMany(x => x.Value))
            },

            NotFoundException notFoundEx => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = ApiResponse<object>.FailureResult(notFoundEx.Message)
            },

            DomainException domainEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = ApiResponse<object>.FailureResult(domainEx.Message)
            },

            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = ApiResponse<object>.FailureResult(
                    "Ocurrió un error inesperado en el servidor.",
                    _env.IsDevelopment() ? new[] { exception.Message } : null)
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Body, jsonOptions));
    }
}
