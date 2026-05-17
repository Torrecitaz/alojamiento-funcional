using System.Net;
using System.Text.Json;

namespace Microservicio.Clientes.Api.Middleware;

/// <summary>
/// Intercepta todas las excepciones no controladas y las convierte en respuestas JSON
/// con el código HTTP correcto. Sin este middleware, Swagger devolvería HTML en errores.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statusCode = ex switch
        {
            KeyNotFoundException   => HttpStatusCode.NotFound,
            ArgumentException      => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.Conflict,
            _                      => HttpStatusCode.InternalServerError
        };

        var response = new
        {
            exitoso    = false,
            mensaje    = ex.Message,
            statusCode = (int)statusCode,
            traceId    = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
