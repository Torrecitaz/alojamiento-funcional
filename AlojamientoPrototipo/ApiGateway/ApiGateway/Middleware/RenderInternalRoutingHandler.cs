using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ApiGateway.Middleware;

public class RenderInternalRoutingHandler : DelegatingHandler
{
    private readonly ILogger<RenderInternalRoutingHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _wakeUpUrls = new(StringComparer.OrdinalIgnoreCase);

    public RenderInternalRoutingHandler(ILogger<RenderInternalRoutingHandler> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        InitializeWakeUpUrls();
    }

    private void InitializeWakeUpUrls()
    {
        var serviceMappings = new[]
        {
            new { ConfigKey = "Microservices:UsuariosUrl", PublicUrl = "https://usuarios-api-y75a.onrender.com/" },
            new { ConfigKey = "Microservices:AlojamientosUrl", PublicUrl = "https://alojamientos-api-y75a.onrender.com/" },
            new { ConfigKey = "Microservices:ReservasUrl", PublicUrl = "https://reservas-api-y75a.onrender.com/" },
            new { ConfigKey = "Microservices:FacturacionUrl", PublicUrl = "https://facturacion-api-y75a.onrender.com/" }
        };

        foreach (var mapping in serviceMappings)
        {
            var configuredUrl = _configuration[mapping.ConfigKey];
            if (!string.IsNullOrEmpty(configuredUrl))
            {
                try
                {
                    var urlToParse = configuredUrl;
                    if (!urlToParse.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                        !urlToParse.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        urlToParse = "http://" + urlToParse;
                    }

                    var uri = new Uri(urlToParse);
                    var host = uri.Host;
                    if (!string.IsNullOrEmpty(host))
                    {
                        _wakeUpUrls[host] = mapping.PublicUrl;
                        _logger.LogInformation("[WAKEUP] Dynamically mapped private host '{Host}' to public wakeup URL '{PublicUrl}'", host, mapping.PublicUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[WAKEUP] Failed to parse private URL for config {Key} ({Value}): {Message}", 
                        mapping.ConfigKey, configuredUrl, ex.Message);
                }
            }
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestUrl = request.RequestUri?.ToString() ?? "";
        int maxConnectionRetries = 15; // 15 reintentos * 4s = 60s total para cold starts
        int maxRateLimitRetries = 5;

        int connectionAttempt = 0;
        int rateLimitAttempt = 0;

        // Buffer the request content bytes once before any attempts to avoid stream consumption issues
        byte[]? requestContentBytes = null;
        if (request.Content != null)
        {
            requestContentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        while (true)
        {
            try
            {
                // Recreate the request for the current attempt to ensure fresh stream/content
                var attemptRequest = CloneRequest(request, requestContentBytes);
                var response = await base.SendAsync(attemptRequest, cancellationToken);

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
                    continue;
                }

                return response;
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                connectionAttempt++;
                if (connectionAttempt > maxConnectionRetries)
                {
                    _logger.LogError(ex, "Falla de conexión persistentente a {Url} tras {Max} intentos de cold start.", requestUrl, maxConnectionRetries);
                    throw;
                }

                // Try to wake up the service if its host matches a known private Render service name
                var host = request.RequestUri?.Host;
                if (!string.IsNullOrEmpty(host))
                {
                    TryWakeUpService(host);
                }

                _logger.LogWarning("Falla de conexión a {Url} (microservicio durmiendo o cargando). Esperando 4 segundos para reintentar... (Intento {Attempt}/{Max})", 
                    requestUrl, connectionAttempt, maxConnectionRetries);

                await Task.Delay(4000, cancellationToken);
            }
        }
    }

    private void TryWakeUpService(string host)
    {
        if (_wakeUpUrls.TryGetValue(host, out var wakeUpUrl))
        {
            _logger.LogInformation("[WAKEUP] Sending background wake-up ping to public URL '{WakeUpUrl}' for service host '{Host}'", wakeUpUrl, host);
            _ = Task.Run(async () =>
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                    await client.GetAsync(wakeUpUrl);
                }
                catch (Exception ex)
                {
                    // Ignore exceptions since a timeout/error is normal for a sleeping service, but Render will still trigger wakeup
                    _logger.LogDebug("[WAKEUP] Ping to {WakeUpUrl} completed/failed: {Message}", wakeUpUrl, ex.Message);
                }
            });
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

    private HttpRequestMessage CloneRequest(HttpRequestMessage req, byte[]? contentBytes)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri)
        {
            Version = req.Version
        };

        if (contentBytes != null && req.Content != null)
        {
            var newContent = new ByteArrayContent(contentBytes);
            foreach (var header in req.Content.Headers)
            {
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            clone.Content = newContent;
        }

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
