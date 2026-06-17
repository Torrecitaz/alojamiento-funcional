using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Alojamientos.API.Models.Common;
using Alojamientos.Business.Exceptions;

namespace Alojamientos.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
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
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
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

        var innerEx = exception.InnerException;
        var innerMsg = innerEx?.Message;
        if (innerEx != null)
        {
            var detailProp = innerEx.GetType().GetProperty("Detail");
            if (detailProp != null)
            {
                var detailVal = detailProp.GetValue(innerEx) as string;
                if (!string.IsNullOrEmpty(detailVal))
                {
                    innerMsg += $" | Detail: {detailVal}";
                }
            }
        }

        var response = new ApiErrorResponse(
            message: exception.Message,
            details: statusCode == HttpStatusCode.InternalServerError
                ? (innerMsg != null ? $"Inner: {innerMsg}" : "Ocurrió un error interno en el servidor.")
                : null
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
