using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private const string IdempotencyHeaderName = "X-Idempotency-Key";
    private const string AlternateIdempotencyHeaderName = "Idempotency-Key";

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache cache, ILogger<IdempotencyMiddleware> logger)
    {
        // Only apply to POST requests under /api/v2/
        if (context.Request.Method != "POST" || 
            !context.Request.Path.StartsWithSegments("/api/v2", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Try to get the idempotency key header
        string? key = null;
        if (context.Request.Headers.TryGetValue(IdempotencyHeaderName, out var headerValues))
        {
            key = headerValues.FirstOrDefault();
        }
        else if (context.Request.Headers.TryGetValue(AlternateIdempotencyHeaderName, out var altHeaderValues))
        {
            key = altHeaderValues.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogWarning("Missing idempotency key header on request to {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"La cabecera '{IdempotencyHeaderName}' es obligatoria para peticiones transaccionales en V2."
            });
            return;
        }

        // Validate that the key is a valid Guid
        if (!Guid.TryParse(key, out _))
        {
            logger.LogWarning("Invalid format for idempotency key: {Key}", key);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"La cabecera '{IdempotencyHeaderName}' debe ser un UUID/GUID válido."
            });
            return;
        }

        var cacheKey = $"idempotency:{key}";

        if (cache.TryGetValue(cacheKey, out object? cachedValue))
        {
            if (cachedValue is string str && str == "processing")
            {
                logger.LogWarning("Concurrent request detected for idempotency key: {Key}", key);
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Hay otra transacción en proceso con la misma clave de idempotencia."
                });
                return;
            }

            if (cachedValue is IdempotentResponse cachedResponse)
            {
                logger.LogInformation("Replaying cached response for idempotency key: {Key}", key);
                context.Response.StatusCode = cachedResponse.StatusCode;
                context.Response.ContentType = cachedResponse.ContentType;
                await context.Response.WriteAsync(cachedResponse.Body);
                return;
            }
        }

        // Set state to processing
        cache.Set(cacheKey, "processing", TimeSpan.FromMinutes(2)); // Lock for 2 minutes or until finished

        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            if (statusCode >= 200 && statusCode < 300)
            {
                // Cache the response
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(responseBodyStream).ReadToEndAsync();
                
                var idempotentResponse = new IdempotentResponse
                {
                    StatusCode = statusCode,
                    ContentType = context.Response.ContentType ?? "application/json",
                    Body = body
                };

                // Store for 1 hour
                cache.Set(cacheKey, idempotentResponse, TimeSpan.FromHours(1));

                // Copy to original stream
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            else
            {
                // On failure, remove the lock so user can retry
                cache.Remove(cacheKey);
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
        }
        catch (Exception)
        {
            // On exception, remove the lock and rethrow
            cache.Remove(cacheKey);
            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBodyStream;
        }
    }
}

public class IdempotentResponse
{
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = "application/json";
    public string Body { get; set; } = string.Empty;
}
