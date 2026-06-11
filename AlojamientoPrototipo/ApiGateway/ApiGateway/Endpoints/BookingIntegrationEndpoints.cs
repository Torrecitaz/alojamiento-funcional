using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using ApiGateway.Models;

namespace ApiGateway.Endpoints;

public static class BookingIntegrationEndpoints
{
    public static void MapBookingIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        // Helper to forward requests to BookingIntegration.API
        async Task<IResult> ForwardToSyncApi(HttpRequest request, IHttpClientFactory httpClientFactory, IConfiguration config, string subPath)
        {
            if (!request.Headers.TryGetValue("X-Api-Key", out var apiKey) || apiKey != config["BookingIntegration:ApiKey"])
            {
                return Results.Json(ApiResponse<object>.Fail("No autorizado: API Key inválida."), statusCode: 401);
            }

            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
            var bodyText = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (!request.Headers.TryGetValue("X-Signature", out var signature) || !VerifySignature(bodyText, signature, config["BookingIntegration:HmacSecret"] ?? ""))
            {
                return Results.Json(ApiResponse<object>.Fail("No autorizado: Firma inválida."), statusCode: 401);
            }

            try
            {
                var client = httpClientFactory.CreateClient("BookingIntegration");
                var response = await client.PostAsync($"api/sync/webhook/{subPath}", new StringContent(bodyText, Encoding.UTF8, "application/json"));
                
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail($"Error en servicio de integración: {responseContent}"), statusCode: (int)response.StatusCode);
                }

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return Results.Json(ApiResponse<JsonElement>.Ok(jsonResponse, "Webhook procesado exitosamente por el servicio de integración."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno de Gateway al redirigir: {ex.Message}"), statusCode: 500);
            }
        }

        // 1. Reservation Created Webhook
        app.MapPost("/api/integrations/booking/reservation-created", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
        {
            return await ForwardToSyncApi(request, httpClientFactory, config, "reservation-created");
        })
        .WithName("BookingIntegrationReservationCreated")
        .WithTags("Integraciones")
        .WithOpenApi();

        // 2. Reservation Cancelled Webhook
        app.MapPost("/api/integrations/booking/reservation-cancelled", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
        {
            return await ForwardToSyncApi(request, httpClientFactory, config, "reservation-cancelled");
        })
        .WithName("BookingIntegrationReservationCancelled")
        .WithTags("Integraciones")
        .WithOpenApi();

        // 3. Property Created Webhook
        app.MapPost("/api/integrations/booking/property-created", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
        {
            return await ForwardToSyncApi(request, httpClientFactory, config, "property-created");
        })
        .WithName("BookingIntegrationPropertyCreated")
        .WithTags("Integraciones")
        .WithOpenApi();

        // 4. Property Updated Webhook
        app.MapPost("/api/integrations/booking/property-updated", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
        {
            return await ForwardToSyncApi(request, httpClientFactory, config, "property-updated");
        })
        .WithName("BookingIntegrationPropertyUpdated")
        .WithTags("Integraciones")
        .WithOpenApi();
    }

    private static bool VerifySignature(string bodyText, string? signatureHeader, string secret)
    {
        if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(secret)) return false;
        
        var sig = signatureHeader.Trim();
        if (sig.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            sig = sig.Substring(7);
        }

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(bodyText);
        using var hmac = new HMACSHA256(secretBytes);
        var hashBytes = hmac.ComputeHash(bodyBytes);
        var computedSignature = Convert.ToHexString(hashBytes).ToLower();
        
        return computedSignature.Equals(sig, StringComparison.OrdinalIgnoreCase);
    }
}

public record CrearPropiedadFrontendRequest(
    string Nombre,
    string Descripcion,
    string Direccion,
    int CiudadId,
    int TipoAlojamientoId,
    int Estrellas,
    bool AdmiteMascotas,
    int ColaboradorId,
    string? Provincia = null,
    string? Pais = null,
    string? Politicas = null,
    string? CheckInTime = null,
    string? CheckOutTime = null,
    string? Servicios = null,
    double? Latitud = null,
    double? Longitud = null
);
