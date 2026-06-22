using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Middleware;

public class RenderInternalRoutingHandler : DelegatingHandler
{
    private readonly ILogger<RenderInternalRoutingHandler> _logger;

    public RenderInternalRoutingHandler(ILogger<RenderInternalRoutingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestUrl = request.RequestUri?.ToString() ?? "";
        int maxConnectionRetries = 15; // 15 reintentos * 4s = 60s total para cold starts
        int maxRateLimitRetries = 5;

        int connectionAttempt = 0;
        int rateLimitAttempt = 0;

        while (true)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                // Si detectamos 429 Too Many Requests de Render/Cloudflare, reintentar con backoff
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    rateLimitAttempt++;
                    if (rateLimitAttempt > maxRateLimitRetries)
                    {
                        _logger.LogError("Petición a {Url} falló con 429 Too Many Requests tras {Max} reintentos de rate-limit.", requestUrl, maxRateLimitRetries);
                        return response;
                    }

                    // Determinar tiempo de espera respetando Retry-After si viene en la cabecera
                    double delaySeconds = Math.Pow(2, rateLimitAttempt); // 2s, 4s, 8s, 16s...
                    if (response.Headers.RetryAfter != null)
                    {
                        if (response.Headers.RetryAfter.Delta.HasValue)
                        {
                            delaySeconds = response.Headers.RetryAfter.Delta.Value.TotalSeconds;
                        }
                        else if (response.Headers.RetryAfter.Date.HasValue)
                        {
                            delaySeconds = (response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow).TotalSeconds;
                        }
                    }

                    if (delaySeconds <= 0 || delaySeconds > 30)
                    {
                        delaySeconds = Math.Pow(2, rateLimitAttempt);
                    }

                    _logger.LogWarning("La petición a {Url} retornó 429 (Too Many Requests). Reintentando en {Delay} segundos (Intento {Attempt}/{Max})...", 
                        requestUrl, delaySeconds, rateLimitAttempt, maxRateLimitRetries);

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);

                    // Recrear la petición para el reintento
                    request = CloneRequest(request);
                    continue;
                }

                return response;
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                connectionAttempt++;
                if (connectionAttempt > maxConnectionRetries)
                {
                    _logger.LogError(ex, "Falla de conexión persistente a {Url} tras {Max} intentos de cold start.", requestUrl, maxConnectionRetries);
                    throw;
                }

                _logger.LogWarning("Falla de conexión a {Url} (microservicio durmiendo o cargando). Esperando 4 segundos para reintentar... (Intento {Attempt}/{Max})", 
                    requestUrl, connectionAttempt, maxConnectionRetries);

                await Task.Delay(4000, cancellationToken);

                // Recrear la petición para el reintento
                request = CloneRequest(request);
            }
        }
    }

    private bool IsConnectionFailure(Exception ex)
    {
        return ex is HttpRequestException ||
               ex is System.Net.Sockets.SocketException ||
               ex is TaskCanceledException ||
               ex.InnerException is System.Net.Sockets.SocketException ||
               ex.InnerException is System.IO.IOException;
    }

    private HttpRequestMessage CloneRequest(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri)
        {
            Content = req.Content,
            Version = req.Version
        };

        foreach (var header in req.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var property in req.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(property.Key), property.Value);
        }

        return clone;
    }
}
