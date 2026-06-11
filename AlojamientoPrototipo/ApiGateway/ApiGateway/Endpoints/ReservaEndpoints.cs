using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ApiGateway.Models;
using ApiGateway.Models.Internal;

namespace ApiGateway.Endpoints;

public static class ReservaEndpoints
{
    public static void MapReservaEndpoints(this IEndpointRouteBuilder app)
    {
        // 8. Crear reserva
        app.MapPost("/api/reservas", async (
            CrearReservaRequest request,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                if (request.FechaCheckOut <= request.FechaCheckIn)
                {
                    return Results.Json(ApiResponse<ReservaDto>.Fail("La fecha de check-out debe ser posterior a la de check-in."), statusCode: 400);
                }
                
                var numNoches = request.FechaCheckOut.DayNumber - request.FechaCheckIn.DayNumber;
                
                var internalHabitaciones = request.Habitaciones.Select(h => new
                {
                    habitacionId = h.HabitacionId,
                    precioPorNoche = h.PrecioPorNoche,
                    numNoches = numNoches
                }).ToList();
                
                var internalReq = new
                {
                    clienteId = request.ClienteId,
                    alojamientoId = request.AlojamientoId,
                    fechaCheckIn = request.FechaCheckIn,
                    fechaCheckOut = request.FechaCheckOut,
                    numAdultos = request.NumAdultos,
                    numNinos = request.NumNinos,
                    llevaMascotas = request.LlevaMascotas,
                    codigoDescuento = request.CodigoDescuento,
                    habitaciones = internalHabitaciones,
                    externalId = request.ExternalId
                };
                
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                var fechaFinExclusiva = request.FechaCheckOut.AddDays(-1);

                var reservasClient = httpClientFactory.CreateClient("Reservas");
                HttpResponseMessage response;
                try
                {
                    response = await reservasClient.PostAsJsonAsync("api/v1/Reservas", internalReq);
                }
                catch (Exception ex)
                {
                    // Compensación: liberar fechas si falla la conexión al MS Reservas
                    foreach (var hab in request.Habitaciones)
                    {
                        var liberarReq = new
                        {
                            habitacionId = hab.HabitacionId,
                            fechaInicio = request.FechaCheckIn,
                            fechaFin = fechaFinExclusiva
                        };
                        await alojamientosClient.PostAsJsonAsync("api/v1/Calendario/liberar", liberarReq);
                    }
                    return Results.Json(ApiResponse<ReservaDto>.Fail($"Error de comunicación al crear reserva: {ex.Message}"), statusCode: 500);
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errContent = await response.Content.ReadAsStringAsync();
                    // Compensación: liberar fechas si la creación falló en Reservas
                    foreach (var hab in request.Habitaciones)
                    {
                        var liberarReq = new
                        {
                            habitacionId = hab.HabitacionId,
                            fechaInicio = request.FechaCheckIn,
                            fechaFin = fechaFinExclusiva
                        };
                        await alojamientosClient.PostAsJsonAsync("api/v1/Calendario/liberar", liberarReq);
                    }
                    return Results.Json(ApiResponse<ReservaDto>.Fail($"Error al crear reserva: {errContent}"), statusCode: (int)response.StatusCode);
                }
                
                var internalRes = await response.Content.ReadFromJsonAsync<ReservaInternalResponse>();
                if (internalRes == null)
                {
                    // Compensación: liberar fechas si no pudimos leer la respuesta de la reserva
                    foreach (var hab in request.Habitaciones)
                    {
                        var liberarReq = new
                        {
                            habitacionId = hab.HabitacionId,
                            fechaInicio = request.FechaCheckIn,
                            fechaFin = fechaFinExclusiva
                        };
                        await alojamientosClient.PostAsJsonAsync("api/v1/Calendario/liberar", liberarReq);
                    }
                    return Results.Json(ApiResponse<ReservaDto>.Fail("No se pudo obtener la reserva creada del microservicio."), statusCode: 500);
                }
                
                // Resolve accommodation name
                string nombreAlojamiento = "Alojamiento";
                var accommodationResponse = await alojamientosClient.GetAsync($"api/v1/Alojamientos/{request.AlojamientoId}");
                if (accommodationResponse.IsSuccessStatusCode)
                {
                    var accommodation = await accommodationResponse.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                    if (accommodation != null)
                    {
                        nombreAlojamiento = accommodation.Nombre;
                    }
                }

                // Resolve client name
                string nombreCliente = "Cliente";
                var usuariosClient = httpClientFactory.CreateClient("Usuarios");
                var clientResponse = await usuariosClient.GetAsync($"api/v1/Clientes/{request.ClienteId}");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var client = await clientResponse.Content.ReadFromJsonAsync<ClienteInternalResponse>();
                    if (client != null)
                    {
                        nombreCliente = client.Usuario?.NombreCompleto ?? "Cliente";
                    }
                }
                
                var mappedDto = new ReservaDto
                {
                    ReservaId = internalRes.ReservaId,
                    CodigoReserva = internalRes.CodigoReserva,
                    AlojamientoId = internalRes.AlojamientoId,
                    NombreAlojamiento = nombreAlojamiento,
                    NombrePropiedad = nombreAlojamiento,
                    NombreCliente = nombreCliente,
                    FechaCheckIn = internalRes.FechaCheckIn,
                    FechaCheckOut = internalRes.FechaCheckOut,
                    NumNoches = numNoches,
                    NumAdultos = internalRes.NumAdultos,
                    NumNinos = internalRes.NumNinos,
                    LlevaMascotas = internalRes.LlevaMascotas,
                    NumHabitaciones = internalRes.NumHabitaciones,
                    SubTotal = internalRes.SubTotal,
                    Descuento = internalRes.SubTotal - internalRes.Total,
                    Total = internalRes.Total,
                    Moneda = "USD",
                    Estado = internalRes.Estado,
                    FechaCreacion = internalRes.FechaCreacion
                };
                
                return Results.Ok(ApiResponse<ReservaDto>.Ok(mappedDto, "Reserva creada exitosamente"));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<ReservaDto>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        })
        .WithName("CrearReserva")
        .WithTags("Reservas")
        .WithOpenApi();

        // 9. Consultar reserva por código
        app.MapGet("/api/reservas/{codigoReserva}", async (
            string codigoReserva,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var reservasClient = httpClientFactory.CreateClient("Reservas");
                var response = await reservasClient.GetAsync($"api/v1/Reservas/codigo/{codigoReserva}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Results.Json(ApiResponse<ReservaDto>.Fail("Código de reserva no existe."), statusCode: 404);
                }
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<ReservaDto>.Fail($"Error al obtener reserva: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
                }
                
                var internalRes = await response.Content.ReadFromJsonAsync<ReservaInternalResponse>();
                if (internalRes == null)
                {
                    return Results.Json(ApiResponse<ReservaDto>.Fail("Reserva no encontrada."), statusCode: 404);
                }
                
                string nombreAlojamiento = "Alojamiento";
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                var accommodationResponse = await alojamientosClient.GetAsync($"api/v1/Alojamientos/{internalRes.AlojamientoId}");
                if (accommodationResponse.IsSuccessStatusCode)
                {
                    var accommodation = await accommodationResponse.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                    if (accommodation != null)
                    {
                        nombreAlojamiento = accommodation.Nombre;
                    }
                }

                string nombreCliente = "Cliente";
                var usuariosClient = httpClientFactory.CreateClient("Usuarios");
                var clientResponse = await usuariosClient.GetAsync($"api/v1/Clientes/{internalRes.ClienteId}");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var client = await clientResponse.Content.ReadFromJsonAsync<ClienteInternalResponse>();
                    if (client != null)
                    {
                        nombreCliente = client.Usuario?.NombreCompleto ?? "Cliente";
                    }
                }
                
                var numNoches = internalRes.FechaCheckOut.DayNumber - internalRes.FechaCheckIn.DayNumber;
                
                var mappedDto = new ReservaDto
                {
                    ReservaId = internalRes.ReservaId,
                    CodigoReserva = internalRes.CodigoReserva,
                    AlojamientoId = internalRes.AlojamientoId,
                    NombreAlojamiento = nombreAlojamiento,
                    NombrePropiedad = nombreAlojamiento,
                    NombreCliente = nombreCliente,
                    FechaCheckIn = internalRes.FechaCheckIn,
                    FechaCheckOut = internalRes.FechaCheckOut,
                    NumNoches = numNoches,
                    NumAdultos = internalRes.NumAdultos,
                    NumNinos = internalRes.NumNinos,
                    LlevaMascotas = internalRes.LlevaMascotas,
                    NumHabitaciones = internalRes.NumHabitaciones,
                    SubTotal = internalRes.SubTotal,
                    Descuento = internalRes.SubTotal - internalRes.Total,
                    Total = internalRes.Total,
                    Moneda = "USD",
                    Estado = internalRes.Estado,
                    FechaCreacion = internalRes.FechaCreacion
                };
                
                return Results.Ok(ApiResponse<ReservaDto>.Ok(mappedDto));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<ReservaDto>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        })
        .WithName("GetReservaByCodigo")
        .WithTags("Reservas")
        .WithOpenApi();

        // 10. Historial de reservas del huésped
        app.MapGet("/api/reservas/cliente/{clienteId:int}", async (
            int clienteId,
            [FromQuery] string? estado,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var reservasClient = httpClientFactory.CreateClient("Reservas");
                var response = await reservasClient.GetAsync($"api/v1/Reservas/cliente/{clienteId}");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<List<ReservaDto>>.Fail($"Error al obtener reservas: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
                }
                
                var rawList = await response.Content.ReadFromJsonAsync<List<ReservaInternalResponse>>();
                if (rawList == null)
                {
                    return Results.Ok(ApiResponse<List<ReservaDto>>.Ok(new()));
                }
                
                var filteredList = rawList.AsEnumerable();
                if (!string.IsNullOrEmpty(estado))
                {
                    filteredList = filteredList.Where(r => r.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
                }
                
                var listToMap = filteredList.ToList();
                var mappedList = new List<ReservaDto>();
                var accommodationNamesCache = new Dictionary<int, string>();
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");

                string nombreCliente = "Cliente";
                var usuariosClient = httpClientFactory.CreateClient("Usuarios");
                var clientResponse = await usuariosClient.GetAsync($"api/v1/Clientes/{clienteId}");
                if (clientResponse.IsSuccessStatusCode)
                {
                    var client = await clientResponse.Content.ReadFromJsonAsync<ClienteInternalResponse>();
                    if (client != null)
                    {
                        nombreCliente = client.Usuario?.NombreCompleto ?? "Cliente";
                    }
                }
                
                foreach (var r in listToMap)
                {
                    if (!accommodationNamesCache.TryGetValue(r.AlojamientoId, out var name))
                    {
                        name = "Alojamiento";
                        var accRes = await alojamientosClient.GetAsync($"api/v1/Alojamientos/{r.AlojamientoId}");
                        if (accRes.IsSuccessStatusCode)
                        {
                            var acc = await accRes.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                            if (acc != null)
                            {
                                name = acc.Nombre;
                            }
                        }
                        accommodationNamesCache[r.AlojamientoId] = name;
                    }
                    
                    var numNoches = r.FechaCheckOut.DayNumber - r.FechaCheckIn.DayNumber;
                    
                    mappedList.Add(new ReservaDto
                    {
                        ReservaId = r.ReservaId,
                        CodigoReserva = r.CodigoReserva,
                        AlojamientoId = r.AlojamientoId,
                        NombreAlojamiento = name,
                        NombrePropiedad = name,
                        NombreCliente = nombreCliente,
                        FechaCheckIn = r.FechaCheckIn,
                        FechaCheckOut = r.FechaCheckOut,
                        NumNoches = numNoches,
                        NumAdultos = r.NumAdultos,
                        NumNinos = r.NumNinos,
                        LlevaMascotas = r.LlevaMascotas,
                        NumHabitaciones = r.NumHabitaciones,
                        SubTotal = r.SubTotal,
                        Descuento = r.SubTotal - r.Total,
                        Total = r.Total,
                        Moneda = "USD",
                        Estado = r.Estado,
                        FechaCreacion = r.FechaCreacion
                    });
                }
                
                return Results.Ok(ApiResponse<List<ReservaDto>>.Ok(mappedList));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<List<ReservaDto>>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        })
        .WithName("GetReservasByCliente")
        .WithTags("Reservas")
        .WithOpenApi();

        // 11. Cancelar reserva (INVERTED ORDER: state update first, release dates second)
        app.MapPatch("/api/reservas/{id:int}/cancelar", async (
            int id,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var reservasClient = httpClientFactory.CreateClient("Reservas");
                
                var resResponse = await reservasClient.GetAsync($"api/v1/Reservas/{id}");
                if (resResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Results.Json(ApiResponse<object>.Fail("Reserva no encontrada."), statusCode: 404);
                }
                if (!resResponse.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail($"Error al buscar reserva: {resResponse.ReasonPhrase}"), statusCode: (int)resResponse.StatusCode);
                }
                
                var reservation = await resResponse.Content.ReadFromJsonAsync<ReservaInternalResponse>();
                if (reservation == null)
                {
                    return Results.Json(ApiResponse<object>.Fail("Reserva no encontrada."), statusCode: 404);
                }
                
                // 1. Confirmar la cancelación actualizando el estado a "Cancelada" en Reservas
                var statusReq = new { estado = "Cancelada" };
                var patchResponse = await reservasClient.PatchAsJsonAsync($"api/v1/Reservas/{id}/estado", statusReq);
                
                if (!patchResponse.IsSuccessStatusCode)
                {
                    var errContent = await patchResponse.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al actualizar estado en el microservicio: {errContent}"), statusCode: (int)patchResponse.StatusCode);
                }

                // 2. Si la cancelación fue exitosa, liberar las fechas en el calendario
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
                
                return Results.Ok(ApiResponse<object>.Ok(null, "Reserva cancelada con éxito"));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        })
        .WithName("CancelarReserva")
        .WithTags("Reservas")
        .WithOpenApi();
    }
}
