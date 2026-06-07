using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApiGateway.Models;
using ApiGateway.Models.Internal;

namespace ApiGateway.Middleware;

public class CheckoutMiddleware
{
    private readonly RequestDelegate _next;

    public CheckoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "POST" &&
            context.Request.Path.StartsWithSegments("/api/v1/reservas/checkout", StringComparison.OrdinalIgnoreCase))
        {
            var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger<CheckoutMiddleware>();
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            CheckoutBookingRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<CheckoutBookingRequest>(
                    context.Request.Body, jsonOptions);
            }
            catch
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, estado = "FALLIDA_PROVEEDOR", mensaje = "Payload inválido." });
                return;
            }

            if (request == null || string.IsNullOrWhiteSpace(request.IdCarrito) || string.IsNullOrWhiteSpace(request.MetodoPagoId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { success = false, estado = "FALLIDA_PROVEEDOR", mensaje = "idCarrito y metodoPagoId son requeridos." });
                return;
            }

            logger.LogInformation("[CHECKOUT] idCarrito={IdCarrito} metodoPagoId={MetodoPagoId}", request.IdCarrito, request.MetodoPagoId);

            // ── 1. Buscar reserva por ID (int) o por CodigoReserva ──────────────────
            var reservasClient = httpClientFactory.CreateClient("Reservas");
            ReservaInternalResponse? internalRes = null;

            // Intento 1: como ReservaId (int)
            if (int.TryParse(request.IdCarrito, out var reservaIdInt))
            {
                try
                {
                    var resById = await reservasClient.GetAsync($"api/v1/Reservas/{reservaIdInt}");
                    if (resById.IsSuccessStatusCode)
                        internalRes = await resById.Content.ReadFromJsonAsync<ReservaInternalResponse>(jsonOptions);
                    logger.LogInformation("[CHECKOUT] Busqueda por ID={Id}: {Status}", reservaIdInt, resById.StatusCode);
                }
                catch (Exception ex) { logger.LogWarning("[CHECKOUT] Error buscando por ID: {Err}", ex.Message); }
            }

            // Intento 2: como CodigoReserva (RES-...)
            if (internalRes == null)
            {
                try
                {
                    var resByCodigo = await reservasClient.GetAsync($"api/v1/Reservas/codigo/{Uri.EscapeDataString(request.IdCarrito)}");
                    if (resByCodigo.IsSuccessStatusCode)
                        internalRes = await resByCodigo.Content.ReadFromJsonAsync<ReservaInternalResponse>(jsonOptions);
                    logger.LogInformation("[CHECKOUT] Busqueda por codigo={Codigo}: {Status}", request.IdCarrito, resByCodigo.StatusCode);
                }
                catch (Exception ex) { logger.LogWarning("[CHECKOUT] Error buscando por codigo: {Err}", ex.Message); }
            }

            if (internalRes == null)
            {
                logger.LogError("[CHECKOUT] No se encontró reserva para idCarrito={IdCarrito}. Verifique que la reserva existe en el sistema.", request.IdCarrito);
                context.Response.StatusCode = 404;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    estado = "FALLIDA_PROVEEDOR",
                    mensaje = $"Reserva no encontrada. idCarrito '{request.IdCarrito}' no corresponde a ninguna reserva. Use el ReservaId (número) o CodigoReserva (RES-YYYYMMDD-XXXX).",
                    reserva_id = request.IdCarrito
                });
                return;
            }

            logger.LogInformation("[CHECKOUT] Reserva encontrada: ReservaId={Id} Total={Total}", internalRes.ReservaId, internalRes.Total);

            // ── 2. Calcular monto = suma exacta de detalles (Facturación valida esto) ──
            decimal monto;
            object[] detallesPayload;

            if (internalRes.DetallesHabitacion != null && internalRes.DetallesHabitacion.Count > 0)
            {
                var detalles = internalRes.DetallesHabitacion.Select(d => new
                {
                    descripcion = $"Habitación {d.HabitacionId} - {d.NumNoches} noche(s)",
                    cantidad = d.NumNoches,
                    precioUnitario = d.PrecioPorNoche
                }).ToArray();
                monto = detalles.Sum(d => (decimal)d.cantidad * d.precioUnitario);
                detallesPayload = detalles.Cast<object>().ToArray();
            }
            else
            {
                monto = internalRes.Total > 0 ? internalRes.Total : 1m;
                detallesPayload = new object[]
                {
                    new { descripcion = $"Pago Reserva {internalRes.CodigoReserva ?? internalRes.ReservaId.ToString()}", cantidad = 1, precioUnitario = monto }
                };
            }

            logger.LogInformation("[CHECKOUT] Monto calculado={Monto} Detalles={Count}", monto, detallesPayload.Length);

            // ── 3. Crear factura (fechaPago = UtcNow → estado Pagado directo, sin llamar /aprobar) ──
            var facturacionClient = httpClientFactory.CreateClient("Facturacion");
            var crearFacturaPayload = new
            {
                reservaId = internalRes.ReservaId,
                metodoPagoExternalId = request.MetodoPagoId,
                monto,
                fechaPago = DateTime.UtcNow,
                detalles = detallesPayload
            };

            HttpResponseMessage factResponse;
            try
            {
                factResponse = await facturacionClient.PostAsJsonAsync("api/v1/Facturas", crearFacturaPayload);
            }
            catch (Exception ex)
            {
                logger.LogError("[CHECKOUT] Excepcion llamando Facturacion: {Err}", ex.Message);
                context.Response.StatusCode = 502;
                await context.Response.WriteAsJsonAsync(new { success = false, estado = "FALLIDA_PROVEEDOR", mensaje = $"Facturacion no disponible: {ex.Message}", reserva_id = internalRes.ReservaId.ToString() });
                return;
            }

            if (!factResponse.IsSuccessStatusCode)
            {
                var errBody = await factResponse.Content.ReadAsStringAsync();
                logger.LogError("[CHECKOUT] Facturacion respondio {Status}: {Body}", (int)factResponse.StatusCode, errBody);
                context.Response.StatusCode = 502;
                await context.Response.WriteAsJsonAsync(new { success = false, estado = "FALLIDA_PROVEEDOR", mensaje = $"Error al crear factura ({(int)factResponse.StatusCode}): {errBody}", reserva_id = internalRes.ReservaId.ToString() });
                return;
            }

            FacturaInternalResponse? factura;
            try
            {
                factura = await factResponse.Content.ReadFromJsonAsync<FacturaInternalResponse>(jsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogError("[CHECKOUT] Error deserializando factura: {Err}", ex.Message);
                context.Response.StatusCode = 502;
                await context.Response.WriteAsJsonAsync(new { success = false, estado = "FALLIDA_PROVEEDOR", mensaje = "Error al leer respuesta de factura.", reserva_id = internalRes.ReservaId.ToString() });
                return;
            }

            if (factura == null)
            {
                context.Response.StatusCode = 502;
                await context.Response.WriteAsJsonAsync(new { success = false, estado = "FALLIDA_PROVEEDOR", mensaje = "Factura nula tras creación.", reserva_id = internalRes.ReservaId.ToString() });
                return;
            }

            logger.LogInformation("[CHECKOUT] Factura creada: FacturaId={Id}", factura.FacturaId);

            // ── 4. Respuesta exitosa ──────────────────────────────────────────────
            context.Response.StatusCode = 200;
            await context.Response.WriteAsJsonAsync(new CheckoutBookingResponse
            {
                ReservaId = internalRes.ReservaId,
                CodigoReserva = internalRes.CodigoReserva,
                FacturaId = factura.FacturaId,
                Monto = monto,
                Moneda = request.Currency,
                Estado = "COMPLETADO"
            });
            return;
        }

        await _next(context);
    }
}
