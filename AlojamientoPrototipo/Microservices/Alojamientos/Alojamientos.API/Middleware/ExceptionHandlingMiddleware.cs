using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Alojamientos.API.Models.Common;
using Alojamientos.Business.Exceptions;

namespace Alojamientos.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            AlojamientoNotFoundException => HttpStatusCode.NotFound,
            HabitacionNotFoundException => HttpStatusCode.NotFound,
            FotoNotFoundException => HttpStatusCode.NotFound,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        var response = new ApiErrorResponse(
            message: exception.Message,
            details: statusCode == HttpStatusCode.InternalServerError ? "Ocurrió un error interno en el servidor." : null
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
