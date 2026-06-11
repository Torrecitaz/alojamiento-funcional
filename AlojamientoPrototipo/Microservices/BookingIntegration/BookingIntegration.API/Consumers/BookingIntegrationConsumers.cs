using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BookingIntegration.API.Controllers;
using BookingIntegration.API.Data;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Events;

namespace BookingIntegration.API.Consumers;

// ── 1. Reserva Confirmed Consumer ────────────────────────────────
public class BookingSyncReservaConfirmedConsumer : IConsumer<ReservaConfirmedEvent>
{
    private readonly BookingDbHelper _dbHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BookingSyncReservaConfirmedConsumer> _logger;

    public BookingSyncReservaConfirmedConsumer(
        BookingDbHelper dbHelper,
        IHttpClientFactory httpClientFactory,
        ILogger<BookingSyncReservaConfirmedConsumer> logger)
    {
        _dbHelper = dbHelper;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaConfirmedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Event ReservaConfirmedEvent received. Syncing reservation {ReservaId} / {Codigo} to Booking...", msg.ReservaId, msg.CodigoReserva);

        try
        {
            // Fetch complete reservation details from Reservas Microservice
            var client = _httpClientFactory.CreateClient("Reservas");
            var response = await client.GetAsync($"api/v1/Reservas/{msg.ReservaId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch reservation details for {ReservaId} during confirmation sync. Status: {Status}", msg.ReservaId, response.StatusCode);
                return;
            }

            var reserva = await response.Content.ReadFromJsonAsync<ReservaResponse>();
            if (reserva == null)
            {
                _logger.LogError("Reservation details returned empty for {ReservaId} confirm sync", msg.ReservaId);
                return;
            }

            // Block room availability in db_booking (set cupos = 0)
            var fechaFinExclusiva = reserva.FechaCheckOut.AddDays(-1);
            foreach (var det in reserva.DetallesHabitacion)
            {
                await _dbHelper.BlockProductAvailabilityRangeAsync(det.HabitacionId, reserva.FechaCheckIn, fechaFinExclusiva, 0);
            }
            _logger.LogInformation("Availability blocked on Booking for reservation {ReservaId} rooms", msg.ReservaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing confirmed reservation {ReservaId} to Booking", msg.ReservaId);
        }
    }
}

// ── 2. Reserva Cancelled Consumer ────────────────────────────────
public class BookingSyncReservaCancelledConsumer : IConsumer<ReservaCancelledEvent>
{
    private readonly BookingDbHelper _dbHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BookingSyncReservaCancelledConsumer> _logger;

    public BookingSyncReservaCancelledConsumer(
        BookingDbHelper dbHelper,
        IHttpClientFactory httpClientFactory,
        ILogger<BookingSyncReservaCancelledConsumer> logger)
    {
        _dbHelper = dbHelper;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaCancelledEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Event ReservaCancelledEvent received. Syncing cancellation {ReservaId} / {Codigo} to Booking...", msg.ReservaId, msg.CodigoReserva);

        try
        {
            // Fetch complete reservation details from Reservas Microservice (even if cancelled, we can read its dates/rooms)
            var client = _httpClientFactory.CreateClient("Reservas");
            var response = await client.GetAsync($"api/v1/Reservas/{msg.ReservaId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch reservation details for {ReservaId} during cancellation sync. Status: {Status}", msg.ReservaId, response.StatusCode);
                return;
            }

            var reserva = await response.Content.ReadFromJsonAsync<ReservaResponse>();
            if (reserva == null)
            {
                _logger.LogError("Reservation details returned empty for {ReservaId} cancel sync", msg.ReservaId);
                return;
            }

            // Clear/release room availability in db_booking (delete availability constraints)
            var fechaFinExclusiva = reserva.FechaCheckOut.AddDays(-1);
            foreach (var det in reserva.DetallesHabitacion)
            {
                await _dbHelper.ClearProductAvailabilityRangeAsync(det.HabitacionId, reserva.FechaCheckIn, fechaFinExclusiva);
            }

            // If reservation has a UUID (ExternalId), cancel it in Booking DB
            // First we need to get the reservation's external id (if any)
            // Wait, does ReservaResponse contain ExternalId? 
            // In our SyncController, ReservaResponse doesn't model ExternalId, but we can query it or if it has external id, we cancel it.
            // Let's call GetByExternalId or check if it exists in db_booking
            // We can check if bookingId exists in Booking's reserves table
            // Wait! In the Reservas microservice database model, there is indeed ExternalId.
            // Let's get the full reservation from the endpoint. Let's see if the endpoint returns the external ID.
            // Wait, does GET /api/v1/Reservas/{id} return ExternalId? 
            // Let's check ReservasMapper.cs to be sure.
            // Yes, ReservasMapper maps ExternalId. But even if it doesn't, we can cancel by bookingId.
            // Let's fetch the raw reservation with a generic JsonElement to read ExternalId if it exists.
            var rawResponse = await client.GetAsync($"api/v1/Reservas/{msg.ReservaId}");
            if (rawResponse.IsSuccessStatusCode)
            {
                var rawJson = await rawResponse.Content.ReadFromJsonAsync<JsonElement>();
                if (rawJson.TryGetProperty("externalId", out var extIdProp) && extIdProp.ValueKind == JsonValueKind.String)
                {
                    if (Guid.TryParse(extIdProp.GetString(), out var bookingId))
                    {
                        await _dbHelper.CancelBookingDbReservationAsync(bookingId);
                        _logger.LogInformation("Cancelled booking {BookingId} in db_booking", bookingId);
                    }
                }
            }

            _logger.LogInformation("Availability cleared on Booking for cancelled reservation {ReservaId}", msg.ReservaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing cancelled reservation {ReservaId} to Booking", msg.ReservaId);
        }
    }
}

// ── 3. Habitacion Availability Changed Consumer ──────────────────
public class BookingSyncHabitacionDisponibilidadChangedConsumer : IConsumer<HabitacionDisponibilidadChangedEvent>
{
    private readonly BookingDbHelper _dbHelper;
    private readonly ILogger<BookingSyncHabitacionDisponibilidadChangedConsumer> _logger;

    public BookingSyncHabitacionDisponibilidadChangedConsumer(
        BookingDbHelper dbHelper,
        ILogger<BookingSyncHabitacionDisponibilidadChangedConsumer> logger)
    {
        _dbHelper = dbHelper;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HabitacionDisponibilidadChangedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Event HabitacionDisponibilidadChangedEvent received. Room {RoomId}, Date {Date}, State {State}", msg.HabitacionId, msg.Fecha, msg.Estado);

        try
        {
            int cupos = msg.Estado.Equals("Disponible", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            await _dbHelper.SetProductAvailabilityAsync(msg.HabitacionId, msg.Fecha, cupos);
            _logger.LogInformation("Availability synchronized on Booking for Room {RoomId} Date {Date} as cupos={Cupos}", msg.HabitacionId, msg.Fecha, cupos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing availability for Room {RoomId} Date {Date} to Booking", msg.HabitacionId, msg.Fecha);
        }
    }
}

// ── 4. Alojamiento Estado Changed Consumer ───────────────────────
public class BookingSyncAlojamientoEstadoChangedConsumer : IConsumer<AlojamientoEstadoChangedEvent>
{
    private readonly BookingDbHelper _dbHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BookingSyncAlojamientoEstadoChangedConsumer> _logger;

    public BookingSyncAlojamientoEstadoChangedConsumer(
        BookingDbHelper dbHelper,
        IHttpClientFactory httpClientFactory,
        ILogger<BookingSyncAlojamientoEstadoChangedConsumer> logger)
    {
        _dbHelper = dbHelper;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AlojamientoEstadoChangedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Event AlojamientoEstadoChangedEvent received. Alojamiento {AlojamientoId}, Status {Status}", msg.AlojamientoId, msg.Estado);

        try
        {
            var alojamientosClient = _httpClientFactory.CreateClient("Alojamientos");
            
            // 1. Fetch Alojamiento details
            var alojResponse = await alojamientosClient.GetAsync($"api/v1/Alojamientos/{msg.AlojamientoId}");
            if (!alojResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch alojamiento details for {AlojamientoId}. Status: {Status}", msg.AlojamientoId, alojResponse.StatusCode);
                return;
            }

            var alojamiento = await alojResponse.Content.ReadFromJsonAsync<AlojamientoQueryDetails>();
            if (alojamiento == null)
            {
                _logger.LogError("Alojamiento details returned empty for {AlojamientoId}", msg.AlojamientoId);
                return;
            }

            // 2. Fetch Rooms
            var roomsResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/alojamiento/{msg.AlojamientoId}");
            if (!roomsResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch rooms for alojamiento {AlojamientoId}. Status: {Status}", msg.AlojamientoId, roomsResponse.StatusCode);
                return;
            }

            var rooms = await roomsResponse.Content.ReadFromJsonAsync<List<HabitacionResponse>>();
            if (rooms == null || rooms.Count == 0)
            {
                _logger.LogWarning("No rooms found for alojamiento {AlojamientoId}. Nothing to sync to Booking.", msg.AlojamientoId);
                return;
            }

            // 3. Upsert each room as a product in Booking
            bool isAlojamientoActive = msg.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase);
            
            // Fetch first image url if available
            string? firstImageUrl = null;
            var imagesResponse = await alojamientosClient.GetAsync($"api/v1/Fotos/alojamiento/{msg.AlojamientoId}");
            if (imagesResponse.IsSuccessStatusCode)
            {
                var images = await imagesResponse.Content.ReadFromJsonAsync<List<AlojamientoFotoResponse>>();
                if (images != null && images.Count > 0)
                {
                    firstImageUrl = images[0].Url;
                }
            }

            foreach (var room in rooms)
            {
                string productName = $"{alojamiento.Nombre} - {room.Nombre}";
                string productDescription = $"{room.Nombre}. Capacidad y comodidad superior.";
                await _dbHelper.UpsertProductAsync(
                    room.HabitacionId,
                    productName,
                    productDescription,
                    room.PrecioNoche,
                    isAlojamientoActive,
                    firstImageUrl);
            }

            _logger.LogInformation("Successfully synchronized Alojamiento {AlojamientoId} catalog to Booking", msg.AlojamientoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing alojamiento {AlojamientoId} catalog to Booking", msg.AlojamientoId);
        }
    }
}

// ── Helpers DTOs ──────────────────────────────────────────────────
public record AlojamientoQueryDetails(
    int AlojamientoId,
    string Nombre,
    string Descripcion,
    string Ciudad,
    string Estado
);

public record AlojamientoFotoResponse(
    int FotoId,
    int AlojamientoId,
    string Url,
    int Orden
);
