using MassTransit;
using Shared.Kernel.Events;
using System.Text.Json;

namespace ApiGateway.Consumers;

public class BookingSyncReservaCreatedConsumer : IConsumer<ReservaCreatedEvent>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<BookingSyncReservaCreatedConsumer> _logger;

    public BookingSyncReservaCreatedConsumer(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<BookingSyncReservaCreatedConsumer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaCreatedEvent> context)
    {
        var bookingUrl = _config["BookingIntegration:BookingApiUrl"];
        if (string.IsNullOrEmpty(bookingUrl))
        {
            _logger.LogWarning("⚠️ BookingApiUrl no configurado. Ignorando sincronización de creación de reserva.");
            return;
        }

        _logger.LogInformation("🔄 Sincronizando creación de reserva {CodigoReserva} con Booking...", context.Message.CodigoReserva);

        try
        {
            using var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                event_type = "RESERVATION_CREATED",
                reservaId = context.Message.ReservaId,
                codigoReserva = context.Message.CodigoReserva,
                alojamientoId = context.Message.AlojamientoId,
                fechaCheckIn = context.Message.FechaCheckIn,
                fechaCheckOut = context.Message.FechaCheckOut,
                total = context.Message.Total
            };

            var bodyText = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{bookingUrl.TrimEnd('/')}/api/sync/reservation-created")
            {
                Content = new StringContent(bodyText, System.Text.Encoding.UTF8, "application/json")
            };

            var apiKey = _config["BookingIntegration:ApiKey"] ?? "";
            var hmacSecret = _config["BookingIntegration:HmacSecret"] ?? "";
            request.Headers.Add("X-Api-Key", apiKey);

            // Firmar con HMAC
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(hmacSecret));
            var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(bodyText));
            var signature = Convert.ToHexString(hashBytes).ToLower();
            request.Headers.Add("X-Signature", $"sha256={signature}");

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Reserva {CodigoReserva} sincronizada con Booking.", context.Message.CodigoReserva);
            }
            else
            {
                _logger.LogError("❌ Error al sincronizar reserva con Booking. Status: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al sincronizar creación de reserva con Booking.");
        }
    }
}

public class BookingSyncReservaCancelledConsumer : IConsumer<ReservaCancelledEvent>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<BookingSyncReservaCancelledConsumer> _logger;

    public BookingSyncReservaCancelledConsumer(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<BookingSyncReservaCancelledConsumer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReservaCancelledEvent> context)
    {
        var bookingUrl = _config["BookingIntegration:BookingApiUrl"];
        if (string.IsNullOrEmpty(bookingUrl))
        {
            _logger.LogWarning("⚠️ BookingApiUrl no configurado. Ignorando sincronización de cancelación.");
            return;
        }

        _logger.LogInformation("🔄 Sincronizando cancelación de reserva {CodigoReserva} con Booking...", context.Message.CodigoReserva);

        try
        {
            using var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                event_type = "RESERVATION_CANCELLED",
                reservaId = context.Message.ReservaId,
                codigoReserva = context.Message.CodigoReserva,
                alojamientoId = context.Message.AlojamientoId
            };

            var bodyText = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{bookingUrl.TrimEnd('/')}/api/sync/reservation-cancelled")
            {
                Content = new StringContent(bodyText, System.Text.Encoding.UTF8, "application/json")
            };

            var apiKey = _config["BookingIntegration:ApiKey"] ?? "";
            var hmacSecret = _config["BookingIntegration:HmacSecret"] ?? "";
            request.Headers.Add("X-Api-Key", apiKey);

            // Firmar con HMAC
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(hmacSecret));
            var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(bodyText));
            var signature = Convert.ToHexString(hashBytes).ToLower();
            request.Headers.Add("X-Signature", $"sha256={signature}");

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Cancelación de reserva {CodigoReserva} sincronizada con Booking.", context.Message.CodigoReserva);
            }
            else
            {
                _logger.LogError("❌ Error al sincronizar cancelación con Booking. Status: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al sincronizar cancelación de reserva con Booking.");
        }
    }
}

public class BookingSyncAvailabilityConsumer : IConsumer<HabitacionDisponibilidadChangedEvent>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<BookingSyncAvailabilityConsumer> _logger;

    public BookingSyncAvailabilityConsumer(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<BookingSyncAvailabilityConsumer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HabitacionDisponibilidadChangedEvent> context)
    {
        var bookingUrl = _config["BookingIntegration:BookingApiUrl"];
        if (string.IsNullOrEmpty(bookingUrl))
        {
            return; // Ignorar si no está configurado sin loggear spam
        }

        try
        {
            using var client = _httpClientFactory.CreateClient();
            var payload = new
            {
                event_type = "AVAILABILITY_CHANGED",
                habitacionId = context.Message.HabitacionId,
                fecha = context.Message.Fecha,
                estado = context.Message.Estado
            };

            var bodyText = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{bookingUrl.TrimEnd('/')}/api/sync/availability-changed")
            {
                Content = new StringContent(bodyText, System.Text.Encoding.UTF8, "application/json")
            };

            var apiKey = _config["BookingIntegration:ApiKey"] ?? "";
            var hmacSecret = _config["BookingIntegration:HmacSecret"] ?? "";
            request.Headers.Add("X-Api-Key", apiKey);

            // Firmar con HMAC
            using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(hmacSecret));
            var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(bodyText));
            var signature = Convert.ToHexString(hashBytes).ToLower();
            request.Headers.Add("X-Signature", $"sha256={signature}");

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("⚠️ Error al sincronizar disponibilidad con Booking. Status: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al sincronizar disponibilidad con Booking.");
        }
    }
}
