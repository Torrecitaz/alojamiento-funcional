using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Middleware;

public class RenderInternalRoutingHandler : DelegatingHandler
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RenderInternalRoutingHandler> _logger;
    private readonly HttpClient _wakeupClient;

    public RenderInternalRoutingHandler(IConfiguration configuration, ILogger<RenderInternalRoutingHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _wakeupClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var requestUrl = request.RequestUri?.ToString() ?? "";

        try
        {
            // Intentar enviar la petición
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (IsConnectionFailure(ex))
        {
            _logger.LogWarning("Falla de conexión detectada hacia {Url}. El microservicio podría estar inactivo (durmiendo). Iniciando despertar público...", requestUrl);

            // Obtener la URL pública del microservicio para despertarlo
            var publicUrl = GetPublicUrlForRequest(request.RequestUri);
            if (!string.IsNullOrEmpty(publicUrl))
            {
                // Disparar ping público de fondo sin bloquear el hilo principal de inmediato
                _ = WakeUpServiceAsync(publicUrl);
            }

            // Bucle de reintento: esperar a que el servicio se levante
            int maxRetries = 12; // 12 reintentos * 5s = 60 segundos total (Render tarda 50s promedio en cold start)
            for (int i = 1; i <= maxRetries; i++)
            {
                _logger.LogInformation("Reintentando petición interna a {Url} (Intento {Attempt}/{Max}) en 5 segundos...", requestUrl, i, maxRetries);
                await Task.Delay(5000, cancellationToken);

                try
                {
                    // Debemos clonar la petición porque HttpRequestMessage no se puede reutilizar directamente
                    var clonedRequest = CloneRequest(request);
                    var response = await base.SendAsync(clonedRequest, cancellationToken);
                    _logger.LogInformation("¡Conexión interna exitosa hacia {Url} en el intento {Attempt}!", requestUrl, i);
                    return response;
                }
                catch (Exception retryEx) when (IsConnectionFailure(retryEx))
                {
                    if (i == maxRetries)
                    {
                        _logger.LogError("No se pudo conectar al microservicio en {Url} tras {Max} intentos de espera.", requestUrl, maxRetries);
                        throw;
                    }
                }
            }

            throw;
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

    private string? GetPublicUrlForRequest(Uri? uri)
    {
        if (uri == null) return null;
        var host = uri.Host;

        // Mapear hosts internos de Render a sus correspondientes URLs públicas de despertar (/health)
        if (host.Contains("usuarios-api-y75a")) return "https://usuarios-api-y75a.onrender.com/health";
        if (host.Contains("alojamientos-api-y75a")) return "https://alojamientos-api-y75a.onrender.com/health";
        if (host.Contains("reservas-api-y75a")) return "https://reservas-api-y75a.onrender.com/health";
        if (host.Contains("facturacion-api-y75a")) return "https://facturacion-api-y75a.onrender.com/health";

        return null;
    }

    private async Task WakeUpServiceAsync(string publicUrl)
    {
        try
        {
            _logger.LogInformation("[WakeUp] Enviando ping público para despertar servicio: {Url}", publicUrl);
            await _wakeupClient.GetAsync(publicUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[WakeUp] El ping público de despertar a {Url} terminó con: {Message}", publicUrl, ex.Message);
        }
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
