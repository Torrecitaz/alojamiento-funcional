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
using ApiGateway.Models.Internal;

namespace ApiGateway.Endpoints;

public static class BookingIntegrationEndpoints
{
    public static void MapBookingIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        // Reservation Created Webhook
        app.MapPost("/api/integrations/booking/reservation-created", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
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
                var bookingReq = JsonSerializer.Deserialize<CrearReservaRequest>(bodyText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (bookingReq == null)
                {
                    return Results.Json(ApiResponse<object>.Fail("Payload de reserva inválido."), statusCode: 400);
                }

                var client = httpClientFactory.CreateClient("Reservas");
                var response = await client.PostAsJsonAsync("api/v1/Reservas", bookingReq);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al registrar reserva en Reservas: {error}"), statusCode: (int)response.StatusCode);
                }

                var internalRes = await response.Content.ReadFromJsonAsync<ReservaInternalResponse>();
                return Results.Json(ApiResponse<ReservaInternalResponse>.Ok(internalRes, "Reserva integrada correctamente desde Booking."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        })
        .WithName("BookingIntegrationReservationCreated")
        .WithTags("Integraciones")
        .WithOpenApi();

        // Reservation Cancelled Webhook (INVERTED ORDER: state update first, release dates second)
        app.MapPost("/api/integrations/booking/reservation-cancelled", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
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
                var cancelPayload = JsonSerializer.Deserialize<CancelWebhookPayload>(bodyText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (cancelPayload == null || string.IsNullOrEmpty(cancelPayload.CodigoReserva))
                {
                    return Results.Json(ApiResponse<object>.Fail("Código de reserva requerido."), statusCode: 400);
                }

                var client = httpClientFactory.CreateClient("Reservas");
                var resResponse = await client.GetAsync($"api/v1/Reservas/codigo/{cancelPayload.CodigoReserva}");
                if (!resResponse.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail("Reserva no encontrada en Reservas."), statusCode: 404);
                }

                var reservation = await resResponse.Content.ReadFromJsonAsync<ReservaInternalResponse>();
                if (reservation == null)
                {
                    return Results.Json(ApiResponse<object>.Fail("Reserva no encontrada."), statusCode: 404);
                }

                // 1. Cancelar en Reservas primero
                var statusReq = new { estado = "Cancelada" };
                var patchResponse = await client.PatchAsJsonAsync($"api/v1/Reservas/{reservation.ReservaId}/estado", statusReq);
                if (!patchResponse.IsSuccessStatusCode)
                {
                    var err = await patchResponse.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al actualizar estado en Reservas: {err}"), statusCode: (int)patchResponse.StatusCode);
                }

                // 2. Si es exitoso, liberar el calendario en Alojamientos
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                var fechaFinExclusiva = reservation.FechaCheckOut.AddDays(-1);
                foreach (var det in reservation.DetallesHabitacion)
                {
                    var releaseReq = new
                    {
                        habitacionId = det.HabitacionId,
                        fechaInicio = reservation.FechaCheckIn,
                        fechaFin = fechaFinExclusiva
                    };
                    await alojamientosClient.PostAsJsonAsync("api/v1/Calendario/liberar", releaseReq);
                }

                return Results.Ok(ApiResponse<object>.Ok(null, "Reserva cancelada correctamente desde Booking."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        })
        .WithName("BookingIntegrationReservationCancelled")
        .WithTags("Integraciones")
        .WithOpenApi();

        // Property Created Webhook
        app.MapPost("/api/integrations/booking/property-created", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
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
                var propReq = JsonSerializer.Deserialize<JsonElement>(bodyText);
                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.PostAsJsonAsync("api/v1/Alojamientos", propReq);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al crear propiedad en Alojamientos: {err}"), statusCode: (int)response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Results.Json(ApiResponse<JsonElement>.Ok(result, "Propiedad creada desde Booking."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        })
        .WithName("BookingIntegrationPropertyCreated")
        .WithTags("Integraciones")
        .WithOpenApi();

        // Property Updated Webhook
        app.MapPost("/api/integrations/booking/property-updated", async (
            HttpRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) =>
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
                var payload = JsonSerializer.Deserialize<PropertyUpdatePayload>(bodyText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (payload == null || payload.AlojamientoId <= 0)
                {
                    return Results.Json(ApiResponse<object>.Fail("AlojamientoId inválido."), statusCode: 400);
                }

                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.PutAsJsonAsync($"api/v1/Alojamientos/{payload.AlojamientoId}", payload.Data);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al actualizar propiedad en Alojamientos: {err}"), statusCode: (int)response.StatusCode);
                }

                return Results.Ok(ApiResponse<object>.Ok(null, "Propiedad actualizada desde Booking."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
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

public record CancelWebhookPayload(string CodigoReserva);
public record PropertyUpdatePayload(int AlojamientoId, JsonElement Data);

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
