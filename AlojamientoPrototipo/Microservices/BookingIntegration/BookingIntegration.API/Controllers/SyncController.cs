using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingIntegration.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookingIntegration.API.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly BookingDbHelper _dbHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        BookingDbHelper dbHelper,
        IHttpClientFactory httpClientFactory,
        ILogger<SyncController> logger)
    {
        _dbHelper = dbHelper;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // =========================================================================
    // SET 1: Webhooks FROM Booking (External Platform) TO AlojaExpress
    // =========================================================================

    [HttpPost("webhook/reservation-created")]
    public async Task<IActionResult> WebhookReservationCreated([FromBody] WebhookReservationCreatedRequest request)
    {
        _logger.LogInformation("[Webhook IN] reservation-created received for Booking ID: {BookingId}", request.BookingId);

        try
        {
            // 1. Resolve room ID from Booking UUID to local Habitación ID
            var localHabitacionId = await _dbHelper.GetLocalHabitacionIdAsync(request.RoomId);
            if (localHabitacionId <= 0)
            {
                return BadRequest(new { success = false, message = $"Room with ID {request.RoomId} is not mapped to any local room." });
            }

            // 2. Resolve client by Email in db_usuarios
            var userClient = _httpClientFactory.CreateClient("Usuarios");
            int localClienteId = 0;
            
            var clientCheckResponse = await userClient.GetAsync($"api/v1/Clientes/email/{Uri.EscapeDataString(request.ClienteEmail)}");
            if (clientCheckResponse.IsSuccessStatusCode)
            {
                var clientResponse = await clientCheckResponse.Content.ReadFromJsonAsync<ClienteQueryResponse>();
                if (clientResponse != null)
                {
                    localClienteId = clientResponse.ClienteId;
                }
            }
            else if (clientCheckResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Register new client in db_usuarios
                var registerPayload = new
                {
                    email = request.ClienteEmail,
                    password = "BookingClient123!", // Standard default password
                    nombreCompleto = $"{request.ClienteNombre} {request.ClienteApellido}".Trim(),
                    telefono = request.ClienteTelefono,
                    domicilio = "Booking.com"
                };

                var registerResponse = await userClient.PostAsJsonAsync("api/v1/Clientes/registrar", registerPayload);
                if (!registerResponse.IsSuccessStatusCode)
                {
                    var errorMsg = await registerResponse.Content.ReadAsStringAsync();
                    return StatusCode((int)registerResponse.StatusCode, new { success = false, message = $"Failed to register client locally: {errorMsg}" });
                }

                // Query again to get the generated ClienteId
                var retryResponse = await userClient.GetAsync($"api/v1/Clientes/email/{Uri.EscapeDataString(request.ClienteEmail)}");
                if (retryResponse.IsSuccessStatusCode)
                {
                    var clientResponse = await retryResponse.Content.ReadFromJsonAsync<ClienteQueryResponse>();
                    if (clientResponse != null)
                    {
                        localClienteId = clientResponse.ClienteId;
                    }
                }
            }
            else
            {
                var error = await clientCheckResponse.Content.ReadAsStringAsync();
                return StatusCode((int)clientCheckResponse.StatusCode, new { success = false, message = $"Error checking client: {error}" });
            }

            if (localClienteId <= 0)
            {
                return StatusCode(500, new { success = false, message = "Could not resolve client ID after registration." });
            }

            // 3. Get room details from Alojamientos microservice to retrieve AlojamientoId and price
            var alojamientosClient = _httpClientFactory.CreateClient("Alojamientos");
            var roomResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/{localHabitacionId}");
            if (!roomResponse.IsSuccessStatusCode)
            {
                var error = await roomResponse.Content.ReadAsStringAsync();
                return StatusCode((int)roomResponse.StatusCode, new { success = false, message = $"Failed to retrieve local room details: {error}" });
            }

            var roomDetails = await roomResponse.Content.ReadFromJsonAsync<HabitacionResponse>();
            if (roomDetails == null)
            {
                return StatusCode(500, new { success = false, message = "Room details returned empty." });
            }

            // 4. Create reservation in db_reservas via Reservas microservice
            var reservasClient = _httpClientFactory.CreateClient("Reservas");
            var numNoches = (request.CheckOut.ToDateTime(TimeOnly.MinValue) - request.CheckIn.ToDateTime(TimeOnly.MinValue)).Days;
            
            var createReservaPayload = new
            {
                clienteId = localClienteId,
                alojamientoId = roomDetails.AlojamientoId,
                fechaCheckIn = request.CheckIn,
                fechaCheckOut = request.CheckOut,
                numAdultos = 1,
                numNinos = 0,
                llevaMascotas = false,
                externalId = request.BookingId,
                habitaciones = new[]
                {
                    new
                    {
                        habitacionId = localHabitacionId,
                        precioPorNoche = roomDetails.PrecioNoche,
                        numNoches = numNoches
                    }
                }
            };

            var createReservaResponse = await reservasClient.PostAsJsonAsync("api/v1/Reservas", createReservaPayload);
            if (!createReservaResponse.IsSuccessStatusCode)
            {
                var error = await createReservaResponse.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = $"Failed to create reservation: {error}" });
            }

            var createdReserva = await createReservaResponse.Content.ReadFromJsonAsync<ReservaResponse>();
            if (createdReserva == null)
            {
                return StatusCode(500, new { success = false, message = "Reservation response returned empty." });
            }

            // 5. Confirm the reservation immediately
            var statusPayload = new { estado = "Confirmada" };
            var confirmResponse = await reservasClient.PatchAsJsonAsync($"api/v1/Reservas/{createdReserva.ReservaId}/estado", statusPayload);
            if (!confirmResponse.IsSuccessStatusCode)
            {
                var error = await confirmResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Reservation created but could not be confirmed: {Error}", error);
            }

            // 6. Record reservation in Booking Database (db_booking)
            await _dbHelper.CreateBookingDbReservationAsync(
                request.BookingId,
                request.RoomId,
                request.CheckIn,
                request.CheckOut,
                request.ClienteNombre,
                request.ClienteApellido,
                request.ClienteEmail,
                request.ClienteTelefono,
                request.Total);

            _logger.LogInformation("Successfully integrated Booking reservation {BookingId} as local reservation {ReservaId}", request.BookingId, createdReserva.ReservaId);
            return Ok(new { success = true, message = "Reservation registered and confirmed successfully.", localReservaId = createdReserva.ReservaId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reservation-created webhook");
            return StatusCode(500, new { success = false, message = $"Internal error: {ex.Message}" });
        }
    }

    [HttpPost("webhook/reservation-cancelled")]
    public async Task<IActionResult> WebhookReservationCancelled([FromBody] WebhookReservationCancelledRequest request)
    {
        _logger.LogInformation("[Webhook IN] reservation-cancelled received for Booking ID: {BookingId}", request.BookingId);

        try
        {
            // 1. Get the local reservation using externalId
            var reservasClient = _httpClientFactory.CreateClient("Reservas");
            var getResponse = await reservasClient.GetAsync($"api/v1/Reservas/external/{request.BookingId}");
            if (!getResponse.IsSuccessStatusCode)
            {
                return NotFound(new { success = false, message = $"Reservation with external ID {request.BookingId} not found." });
            }

            var localReserva = await getResponse.Content.ReadFromJsonAsync<ReservaResponse>();
            if (localReserva == null)
            {
                return StatusCode(500, new { success = false, message = "Failed to deserialize local reservation." });
            }

            // 2. Update status to 'Cancelada'
            var statusPayload = new { estado = "Cancelada" };
            var patchResponse = await reservasClient.PatchAsJsonAsync($"api/v1/Reservas/{localReserva.ReservaId}/estado", statusPayload);
            if (!patchResponse.IsSuccessStatusCode)
            {
                var error = await patchResponse.Content.ReadAsStringAsync();
                return StatusCode((int)patchResponse.StatusCode, new { success = false, message = $"Failed to update reservation status: {error}" });
            }

            // 3. Release dates locally in Alojamientos
            var alojamientosClient = _httpClientFactory.CreateClient("Alojamientos");
            var fechaFinExclusiva = localReserva.FechaCheckOut.AddDays(-1);
            foreach (var det in localReserva.DetallesHabitacion)
            {
                var releasePayload = new
                {
                    habitacionId = det.HabitacionId,
                    fechaInicio = localReserva.FechaCheckIn,
                    fechaFin = fechaFinExclusiva
                };
                var releaseResponse = await alojamientosClient.PostAsJsonAsync("api/v1/Calendario/liberar", releasePayload);
                if (!releaseResponse.IsSuccessStatusCode)
                {
                    var error = await releaseResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to release room {RoomId} dates locally: {Error}", det.HabitacionId, error);
                }
            }

            // 4. Cancel reservation in Booking DB
            await _dbHelper.CancelBookingDbReservationAsync(request.BookingId);

            _logger.LogInformation("Successfully cancelled Booking reservation {BookingId} locally and externally", request.BookingId);
            return Ok(new { success = true, message = "Reservation cancelled successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reservation-cancelled webhook");
            return StatusCode(500, new { success = false, message = $"Internal error: {ex.Message}" });
        }
    }

    [HttpPost("webhook/property-created")]
    public async Task<IActionResult> WebhookPropertyCreated([FromBody] JsonElement request)
    {
        _logger.LogInformation("[Webhook IN] property-created received");
        try
        {
            var client = _httpClientFactory.CreateClient("Alojamientos");
            var response = await client.PostAsJsonAsync("api/v1/Alojamientos", request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { success = false, message = $"Error creating property locally: {err}" });
            }
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return Ok(new { success = true, message = "Property created successfully.", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("webhook/property-updated")]
    public async Task<IActionResult> WebhookPropertyUpdated([FromBody] PropertyUpdatePayload request)
    {
        _logger.LogInformation("[Webhook IN] property-updated received for ID: {AlojamientoId}", request.AlojamientoId);
        try
        {
            var client = _httpClientFactory.CreateClient("Alojamientos");
            var response = await client.PutAsJsonAsync($"api/v1/Alojamientos/{request.AlojamientoId}", request.Data);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { success = false, message = $"Error updating property locally: {err}" });
            }
            return Ok(new { success = true, message = "Property updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // =========================================================================
    // SET 2: Sync Notifications FROM AlojaExpress (Internal MS) TO Booking
    // =========================================================================

    [HttpPost("reservation-created")]
    public async Task<IActionResult> SyncReservationCreated([FromBody] JsonElement payload)
    {
        _logger.LogInformation("[Sync OUT] reservation-created notification received from Gateway");

        try
        {
            // Extract reservaId
            if (!payload.TryGetProperty("reservaId", out var idProp) || idProp.ValueKind != JsonValueKind.Number)
            {
                return BadRequest("reservaId is required");
            }
            int reservaId = idProp.GetInt32();

            // Fetch complete reservation details from Reservas Microservice
            var client = _httpClientFactory.CreateClient("Reservas");
            var response = await client.GetAsync($"api/v1/Reservas/{reservaId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch reservation details for {ReservaId} during confirmation sync. Status: {Status}", reservaId, response.StatusCode);
                return StatusCode((int)response.StatusCode, $"Failed to fetch reservation details: {response.StatusCode}");
            }

            var reserva = await response.Content.ReadFromJsonAsync<ReservaResponse>();
            if (reserva == null)
            {
                return BadRequest("Empty reservation details returned");
            }

            // Block room availability in db_booking (set cupos = 0)
            var fechaFinExclusiva = reserva.FechaCheckOut.AddDays(-1);
            foreach (var det in reserva.DetallesHabitacion)
            {
                await _dbHelper.BlockProductAvailabilityRangeAsync(det.HabitacionId, reserva.FechaCheckIn, fechaFinExclusiva, 0);
            }

            return Ok(new { success = true, message = "Reservation blocked on Booking" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SyncReservationCreated");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("reservation-cancelled")]
    public async Task<IActionResult> SyncReservationCancelled([FromBody] JsonElement payload)
    {
        _logger.LogInformation("[Sync OUT] reservation-cancelled notification received from Gateway");

        try
        {
            // Extract reservaId
            if (!payload.TryGetProperty("reservaId", out var idProp) || idProp.ValueKind != JsonValueKind.Number)
            {
                return BadRequest("reservaId is required");
            }
            int reservaId = idProp.GetInt32();

            // Fetch complete reservation details from Reservas Microservice
            var client = _httpClientFactory.CreateClient("Reservas");
            var response = await client.GetAsync($"api/v1/Reservas/{reservaId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch reservation details for {ReservaId} during cancellation sync. Status: {Status}", reservaId, response.StatusCode);
                return StatusCode((int)response.StatusCode, $"Failed to fetch reservation details: {response.StatusCode}");
            }

            var reserva = await response.Content.ReadFromJsonAsync<ReservaResponse>();
            if (reserva == null)
            {
                return BadRequest("Empty reservation details returned");
            }

            // Clear room availability block on Booking
            var fechaFinExclusiva = reserva.FechaCheckOut.AddDays(-1);
            foreach (var det in reserva.DetallesHabitacion)
            {
                await _dbHelper.ClearProductAvailabilityRangeAsync(det.HabitacionId, reserva.FechaCheckIn, fechaFinExclusiva);
            }

            // Check if reservation was a Booking reservation (externalId exists) and cancel it
            var rawResponse = await client.GetAsync($"api/v1/Reservas/{reservaId}");
            if (rawResponse.IsSuccessStatusCode)
            {
                var rawJson = await rawResponse.Content.ReadFromJsonAsync<JsonElement>();
                if (rawJson.TryGetProperty("externalId", out var extIdProp) && extIdProp.ValueKind == JsonValueKind.String)
                {
                    if (Guid.TryParse(extIdProp.GetString(), out var bookingId))
                    {
                        await _dbHelper.CancelBookingDbReservationAsync(bookingId);
                    }
                }
            }

            return Ok(new { success = true, message = "Reservation cleared on Booking" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SyncReservationCancelled");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("availability-changed")]
    public async Task<IActionResult> SyncAvailabilityChanged([FromBody] JsonElement payload)
    {
        _logger.LogInformation("[Sync OUT] availability-changed notification received from Gateway");

        try
        {
            if (!payload.TryGetProperty("habitacionId", out var habProp) ||
                !payload.TryGetProperty("fecha", out var fechaProp) ||
                !payload.TryGetProperty("estado", out var estadoProp))
            {
                return BadRequest("habitacionId, fecha, and estado are required");
            }

            int habitacionId = habProp.GetInt32();
            DateOnly fecha = DateOnly.Parse(fechaProp.GetString()!);
            string estado = estadoProp.GetString()!;

            int cupos = estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            await _dbHelper.SetProductAvailabilityAsync(habitacionId, fecha, cupos);

            return Ok(new { success = true, message = "Availability updated on Booking" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SyncAvailabilityChanged");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

// ── Webhook Models ────────────────────────────────────────────────
public record WebhookReservationCreatedRequest(
    Guid BookingId,
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string ClienteNombre,
    string ClienteApellido,
    string ClienteEmail,
    string ClienteTelefono,
    decimal Total
);

public record WebhookReservationCancelledRequest(
    Guid BookingId
);

public record PropertyUpdatePayload(
    int AlojamientoId,
    JsonElement Data
);

// ── Microservice Query Models ─────────────────────────────────────
public record ClienteQueryResponse(
    int ClienteId,
    string Email,
    string NombreCompleto
);

public record HabitacionResponse(
    int HabitacionId,
    int AlojamientoId,
    decimal PrecioNoche,
    string Nombre
);

public record ReservaResponse(
    int ReservaId,
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    string CodigoReserva,
    string Estado,
    List<ReservaDetalleResponse> DetallesHabitacion
);

public record ReservaDetalleResponse(
    int DetalleId,
    int HabitacionId,
    decimal PrecioPorNoche,
    int NumNoches
);
