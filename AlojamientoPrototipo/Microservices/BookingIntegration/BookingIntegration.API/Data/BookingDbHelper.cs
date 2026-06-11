using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BookingIntegration.API.Data;

public class BookingDbHelper
{
    private readonly string _connectionString;
    private readonly ILogger<BookingDbHelper> _logger;
    private Guid? _providerId;
    private Guid? _categoryId;

    public BookingDbHelper(IConfiguration configuration, ILogger<BookingDbHelper> logger)
    {
        _connectionString = configuration.GetConnectionString("ConexionBooking") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string ConexionBooking is missing");
        _logger = logger;
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }

    public async Task InitializeAsync()
    {
        try
        {
            using var conn = await GetConnectionAsync();

            // 1. Ensure Provider exists
            using (var cmd = new NpgsqlCommand("SELECT id FROM proveedores WHERE nombre = 'AlojaExpress' LIMIT 1;", conn))
            {
                var val = await cmd.ExecuteScalarAsync();
                if (val != null)
                {
                    _providerId = (Guid)val;
                }
                else
                {
                    var newId = Guid.NewGuid();
                    using var insertCmd = new NpgsqlCommand(
                        "INSERT INTO proveedores (id, nombre, url_api_base, tipo_servicio, activo) VALUES (@id, 'AlojaExpress', 'http://api-gateway:8080', 'hoteles', true);", conn);
                    insertCmd.Parameters.AddWithValue("id", newId);
                    await insertCmd.ExecuteNonQueryAsync();
                    _providerId = newId;
                    _logger.LogInformation("Provider AlojaExpress registered in db_booking with ID {Id}", _providerId);
                }
            }

            // 2. Ensure Category exists
            using (var cmd = new NpgsqlCommand("SELECT id FROM categorias WHERE nombre = 'Hoteles' LIMIT 1;", conn))
            {
                var val = await cmd.ExecuteScalarAsync();
                if (val != null)
                {
                    _categoryId = (Guid)val;
                }
                else
                {
                    var newId = Guid.NewGuid();
                    using var insertCmd = new NpgsqlCommand(
                        "INSERT INTO categorias (id, nombre, descripcion) VALUES (@id, 'Hoteles', 'Servicios de hospedaje y hotelería');", conn);
                    insertCmd.Parameters.AddWithValue("id", newId);
                    await insertCmd.ExecuteNonQueryAsync();
                    _categoryId = newId;
                    _logger.LogInformation("Category 'Hoteles' registered in db_booking with ID {Id}", _categoryId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Booking database metadata");
        }
    }

    public async Task UpsertProductAsync(int habitacionId, string name, string description, decimal price, bool active, string? imageUrl)
    {
        if (!_providerId.HasValue || !_categoryId.HasValue)
        {
            await InitializeAsync();
        }

        try
        {
            using var conn = await GetConnectionAsync();
            
            // Check if product exists
            Guid? productId = null;
            using (var cmd = new NpgsqlCommand("SELECT id FROM productos WHERE id_externo = @id_externo AND id_proveedor = @id_proveedor LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue("id_externo", habitacionId.ToString());
                cmd.Parameters.AddWithValue("id_proveedor", _providerId!.Value);
                var val = await cmd.ExecuteScalarAsync();
                if (val != null) productId = (Guid)val;
            }

            if (productId.HasValue)
            {
                // Update
                using var cmd = new NpgsqlCommand(
                    "UPDATE productos SET nombre = @nombre, descripcion = @descripcion, precio = @precio, disponible = @disponible, imagen_url = @imagen_url WHERE id = @id;", conn);
                cmd.Parameters.AddWithValue("nombre", name);
                cmd.Parameters.AddWithValue("descripcion", description ?? "");
                cmd.Parameters.AddWithValue("precio", price);
                cmd.Parameters.AddWithValue("disponible", active);
                cmd.Parameters.AddWithValue("imagen_url", imageUrl ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("id", productId.Value);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Updated product {HabitacionId} on Booking database", habitacionId);
            }
            else
            {
                // Insert
                var newId = Guid.NewGuid();
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO productos (id, id_proveedor, id_categoria, id_externo, nombre, descripcion, precio, moneda, disponible, imagen_url) VALUES (@id, @id_proveedor, @id_categoria, @id_externo, @nombre, @descripcion, @precio, 'USD', @disponible, @imagen_url);", conn);
                cmd.Parameters.AddWithValue("id", newId);
                cmd.Parameters.AddWithValue("id_proveedor", _providerId!.Value);
                cmd.Parameters.AddWithValue("id_categoria", _categoryId!.Value);
                cmd.Parameters.AddWithValue("id_externo", habitacionId.ToString());
                cmd.Parameters.AddWithValue("nombre", name);
                cmd.Parameters.AddWithValue("descripcion", description ?? "");
                cmd.Parameters.AddWithValue("precio", price);
                cmd.Parameters.AddWithValue("disponible", active);
                cmd.Parameters.AddWithValue("imagen_url", imageUrl ?? (object)DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Created product {HabitacionId} on Booking database with ID {Id}", habitacionId, newId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting product {HabitacionId} on Booking database", habitacionId);
        }
    }

    public async Task SetProductAvailabilityAsync(int habitacionId, DateOnly date, int cupos)
    {
        if (!_providerId.HasValue) await InitializeAsync();

        try
        {
            using var conn = await GetConnectionAsync();
            
            // 1. Get product UUID from id_externo
            Guid? productId = null;
            using (var cmd = new NpgsqlCommand("SELECT id FROM productos WHERE id_externo = @id_externo AND id_proveedor = @id_proveedor LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue("id_externo", habitacionId.ToString());
                cmd.Parameters.AddWithValue("id_proveedor", _providerId!.Value);
                var val = await cmd.ExecuteScalarAsync();
                if (val != null) productId = (Guid)val;
            }

            if (!productId.HasValue)
            {
                _logger.LogWarning("Product with external ID {HabitacionId} not found in Booking. Skipping availability change.", habitacionId);
                return;
            }

            // 2. Upsert availability
            using (var cmd = new NpgsqlCommand(
                @"INSERT INTO disponibilidad_productos (id_producto, fecha, cupos_disponibles)
                  VALUES (@id_producto, @fecha, @cupos)
                  ON CONFLICT (id_producto, fecha)
                  DO UPDATE SET cupos_disponibles = EXCLUDED.cupos_disponibles;", conn))
            {
                cmd.Parameters.AddWithValue("id_producto", productId.Value);
                cmd.Parameters.AddWithValue("fecha", date);
                cmd.Parameters.AddWithValue("cupos", cupos);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for HabitacionId {HabitacionId} on Date {Date}", habitacionId, date);
        }
    }

    public async Task BlockProductAvailabilityRangeAsync(int habitacionId, DateOnly start, DateOnly end, int cupos)
    {
        if (!_providerId.HasValue) await InitializeAsync();

        try
        {
            using var conn = await GetConnectionAsync();
            
            // 1. Get product UUID from id_externo
            Guid? productId = null;
            using (var cmd = new NpgsqlCommand("SELECT id FROM productos WHERE id_externo = @id_externo AND id_proveedor = @id_proveedor LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue("id_externo", habitacionId.ToString());
                cmd.Parameters.AddWithValue("id_proveedor", _providerId!.Value);
                var val = await cmd.ExecuteScalarAsync();
                if (val != null) productId = (Guid)val;
            }

            if (!productId.HasValue)
            {
                _logger.LogWarning("Product with external ID {HabitacionId} not found in Booking. Skipping block.", habitacionId);
                return;
            }

            // 2. Loop dates
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO disponibilidad_productos (id_producto, fecha, cupos_disponibles)
                      VALUES (@id_producto, @fecha, @cupos)
                      ON CONFLICT (id_producto, fecha)
                      DO UPDATE SET cupos_disponibles = EXCLUDED.cupos_disponibles;", conn);
                cmd.Parameters.AddWithValue("id_producto", productId.Value);
                cmd.Parameters.AddWithValue("fecha", d);
                cmd.Parameters.AddWithValue("cupos", cupos);
                await cmd.ExecuteNonQueryAsync();
            }
            _logger.LogInformation("Blocked availability (cupos={Cupos}) on Booking for Habitación {HabitacionId} from {Start} to {End}", cupos, habitacionId, start, end);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking range for HabitacionId {HabitacionId}", habitacionId);
        }
    }

    public async Task ClearProductAvailabilityRangeAsync(int habitacionId, DateOnly start, DateOnly end)
    {
        if (!_providerId.HasValue) await InitializeAsync();

        try
        {
            using var conn = await GetConnectionAsync();
            
            // 1. Get product UUID from id_externo
            Guid? productId = null;
            using (var cmd = new NpgsqlCommand("SELECT id FROM productos WHERE id_externo = @id_externo AND id_proveedor = @id_proveedor LIMIT 1;", conn))
            {
                cmd.Parameters.AddWithValue("id_externo", habitacionId.ToString());
                cmd.Parameters.AddWithValue("id_proveedor", _providerId!.Value);
                var val = await cmd.ExecuteScalarAsync();
                if (val != null) productId = (Guid)val;
            }

            if (!productId.HasValue)
            {
                _logger.LogWarning("Product with external ID {HabitacionId} not found in Booking. Skipping clear.", habitacionId);
                return;
            }

            // 2. Delete rows in range to restore default availability
            using (var cmd = new NpgsqlCommand(
                "DELETE FROM disponibilidad_productos WHERE id_producto = @id_producto AND fecha >= @start AND fecha <= @end;", conn))
            {
                cmd.Parameters.AddWithValue("id_producto", productId.Value);
                cmd.Parameters.AddWithValue("start", start);
                cmd.Parameters.AddWithValue("end", end);
                await cmd.ExecuteNonQueryAsync();
            }
            _logger.LogInformation("Cleared blocks on Booking for Habitación {HabitacionId} from {Start} to {End}", habitacionId, start, end);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing range for HabitacionId {HabitacionId}", habitacionId);
        }
    }

    public async Task<Guid> ResolveRoomUuidToLocalIdAsync(Guid roomId)
    {
        try
        {
            using var conn = await GetConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT id_externo FROM productos WHERE id = @id LIMIT 1;", conn);
            cmd.Parameters.AddWithValue("id", roomId);
            var val = await cmd.ExecuteScalarAsync();
            if (val != null && int.TryParse(val.ToString(), out var localId))
            {
                return roomId; // Validated
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving room UUID {RoomId}", roomId);
        }
        return Guid.Empty;
    }

    public async Task<int> GetLocalHabitacionIdAsync(Guid roomId)
    {
        try
        {
            using var conn = await GetConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT id_externo FROM productos WHERE id = @id LIMIT 1;", conn);
            cmd.Parameters.AddWithValue("id", roomId);
            var val = await cmd.ExecuteScalarAsync();
            if (val != null && int.TryParse(val.ToString(), out var localId))
            {
                return localId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting local HabitacionId for RoomId {RoomId}", roomId);
        }
        return 0;
    }

    public async Task<string> GetProductNameAsync(Guid roomId)
    {
        try
        {
            using var conn = await GetConnectionAsync();
            using var cmd = new NpgsqlCommand("SELECT nombre FROM productos WHERE id = @id LIMIT 1;", conn);
            cmd.Parameters.AddWithValue("id", roomId);
            var val = await cmd.ExecuteScalarAsync();
            return val?.ToString() ?? "Habitación de Booking";
        }
        catch
        {
            return "Habitación de Booking";
        }
    }

    public async Task CreateBookingDbReservationAsync(Guid bookingId, Guid roomId, DateOnly checkIn, DateOnly checkOut, string nombre, string apellido, string email, string telefono, decimal total)
    {
        if (!_providerId.HasValue) await InitializeAsync();

        try
        {
            using var conn = await GetConnectionAsync();
            using var trans = await conn.BeginTransactionAsync();

            try
            {
                // 1. Get or create client in Booking DB
                Guid clientId;
                using (var cmd = new NpgsqlCommand("SELECT id FROM clientes WHERE email = @email LIMIT 1;", conn, trans))
                {
                    cmd.Parameters.AddWithValue("email", email);
                    var val = await cmd.ExecuteScalarAsync();
                    if (val != null)
                    {
                        clientId = (Guid)val;
                    }
                    else
                    {
                        clientId = Guid.NewGuid();
                        using var insertCmd = new NpgsqlCommand(
                            "INSERT INTO clientes (id, nombre, apellido, email, telefono) VALUES (@id, @nombre, @apellido, @email, @telefono);", conn, trans);
                        insertCmd.Parameters.AddWithValue("id", clientId);
                        insertCmd.Parameters.AddWithValue("nombre", nombre);
                        insertCmd.Parameters.AddWithValue("apellido", apellido);
                        insertCmd.Parameters.AddWithValue("email", email);
                        insertCmd.Parameters.AddWithValue("telefono", telefono ?? (object)DBNull.Value);
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                }

                // 2. Ensure default payment method exists
                Guid paymentMethodId;
                using (var cmd = new NpgsqlCommand("SELECT id FROM metodos_pago WHERE nombre = 'Tarjeta de Crédito' LIMIT 1;", conn, trans))
                {
                    var val = await cmd.ExecuteScalarAsync();
                    if (val != null)
                    {
                        paymentMethodId = (Guid)val;
                    }
                    else
                    {
                        paymentMethodId = Guid.NewGuid();
                        using var insertCmd = new NpgsqlCommand(
                            "INSERT INTO metodos_pago (id, nombre) VALUES (@id, 'Tarjeta de Crédito');", conn, trans);
                        insertCmd.Parameters.AddWithValue("id", paymentMethodId);
                        await insertCmd.ExecuteNonQueryAsync();
                    }
                }

                // 3. Get product details (id_externo, nombre)
                string localExternalId = "";
                string productName = "";
                using (var cmd = new NpgsqlCommand("SELECT id_externo, nombre FROM productos WHERE id = @id LIMIT 1;", conn, trans))
                {
                    cmd.Parameters.AddWithValue("id", roomId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        localExternalId = reader.GetString(0);
                        productName = reader.GetString(1);
                    }
                }

                // 4. Insert Reservation in db_booking.reservas
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO reservas (id, id_cliente, estado, total, fecha_reserva) VALUES (@id, @id_cliente, 'confirmada', @total, now());", conn, trans))
                {
                    cmd.Parameters.AddWithValue("id", bookingId);
                    cmd.Parameters.AddWithValue("id_cliente", clientId);
                    cmd.Parameters.AddWithValue("total", total);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 5. Insert Detalle in db_booking.detalles_reserva
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO detalles_reserva (id, id_reserva, id_producto, id_proveedor, id_externo, nombre, cantidad, precio_unitario) VALUES (@id, @id_reserva, @id_producto, @id_proveedor, @id_externo, @nombre, 1, @precio);", conn, trans))
                {
                    cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("id_reserva", bookingId);
                    cmd.Parameters.AddWithValue("id_producto", roomId);
                    cmd.Parameters.AddWithValue("id_proveedor", _providerId!.Value);
                    cmd.Parameters.AddWithValue("id_externo", localExternalId);
                    cmd.Parameters.AddWithValue("nombre", productName);
                    cmd.Parameters.AddWithValue("precio", total);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 6. Insert Payment in db_booking.pagos
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO pagos (id, id_reserva, id_metodo_pago, monto, estado, fecha_pago) VALUES (@id, @id_reserva, @id_metodo_pago, @monto, 'pagado', now());", conn, trans))
                {
                    cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("id_reserva", bookingId);
                    cmd.Parameters.AddWithValue("id_metodo_pago", paymentMethodId);
                    cmd.Parameters.AddWithValue("monto", total);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 7. Block availability in db_booking.disponibilidad_productos
                var numDays = (checkOut.ToDateTime(TimeOnly.MinValue) - checkIn.ToDateTime(TimeOnly.MinValue)).Days;
                for (int i = 0; i < numDays; i++)
                {
                    var date = checkIn.AddDays(i);
                    using var cmd = new NpgsqlCommand(
                        @"INSERT INTO disponibilidad_productos (id_producto, fecha, cupos_disponibles)
                          VALUES (@id_producto, @fecha, 0)
                          ON CONFLICT (id_producto, fecha)
                          DO UPDATE SET cupos_disponibles = 0;", conn, trans);
                    cmd.Parameters.AddWithValue("id_producto", roomId);
                    cmd.Parameters.AddWithValue("fecha", date);
                    await cmd.ExecuteNonQueryAsync();
                }

                await trans.CommitAsync();
                _logger.LogInformation("Simulated Booking reservation {BookingId} successfully created in db_booking", bookingId);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "Transaction failed creating simulated Booking reservation {BookingId}", bookingId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create simulated Booking reservation {BookingId}", bookingId);
            throw;
        }
    }

    public async Task CancelBookingDbReservationAsync(Guid bookingId)
    {
        if (!_providerId.HasValue) await InitializeAsync();

        try
        {
            using var conn = await GetConnectionAsync();
            using var trans = await conn.BeginTransactionAsync();

            try
            {
                // 1. Update reservation state in db_booking.reservas
                using (var cmd = new NpgsqlCommand("UPDATE reservas SET estado = 'cancelada' WHERE id = @id;", conn, trans))
                {
                    cmd.Parameters.AddWithValue("id", bookingId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. Get product and check-in / check-out dates from reservation details to release availability
                // Wait! In db_booking, how do we know check-in/check-out?
                // Let's see: we can query the reservation metadata or details if they are in details.
                // Wait! Booking details might not have dates, but wait! We can query disponibilidad_productos?
                // Ah, let's see. If the webhook tells us to cancel bookingId, the webhook itself doesn't send dates, just bookingId.
                // Let's check how booking cancellation webhook is defined in ApiGateway:
                // `var cancelPayload = JsonSerializer.Deserialize<CancelWebhookPayload>(bodyText);`
                // Wait, it gets the reservation from the local `Reservas` microservice!
                // Yes! In ApiGateway's cancelled webhook:
                // `var resResponse = await client.GetAsync("api/v1/Reservas/codigo/...");` or by externalId.
                // So the local reservation has `FechaCheckIn` and `FechaCheckOut`!
                // Wait, let's look at `db_booking` `detalles_reserva` table. Does it contain the product? Yes, `id_producto`.
                // So if we delete availability in `disponibilidad_productos` on those dates, we need the product ID and the dates.
                // Since the local AlojaExpress reservation has the dates, we can pass the dates from the controller to this method!
                // Yes! Let's modify the method signature to:
                // `CancelBookingDbReservationAsync(Guid bookingId, Guid roomId, DateOnly checkIn, DateOnly checkOut)`

                // 3. Update pagos
                using (var cmd = new NpgsqlCommand("UPDATE pagos SET estado = 'fallido' WHERE id_reserva = @id;", conn, trans))
                {
                    cmd.Parameters.AddWithValue("id", bookingId);
                    await cmd.ExecuteNonQueryAsync();
                }

                await trans.CommitAsync();
                _logger.LogInformation("Simulated Booking reservation {BookingId} successfully cancelled in db_booking", bookingId);
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "Transaction failed cancelling simulated Booking reservation {BookingId}", bookingId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel simulated Booking reservation {BookingId}", bookingId);
            throw;
        }
    }
}
