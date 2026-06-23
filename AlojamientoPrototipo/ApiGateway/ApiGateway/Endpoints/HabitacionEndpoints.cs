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

public static class HabitacionEndpoints
{
    public static void MapHabitacionEndpoints(this IEndpointRouteBuilder app)
    {
        // Core handler for checking availability
        var checkDisponibilidad = async (
            int id,
            string fechaDesde,
            string fechaHasta,
            int adultos,
            int ninos,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                if (!DateOnly.TryParse(fechaDesde, out var dateDesde) || !DateOnly.TryParse(fechaHasta, out var dateHasta))
                {
                    return Results.Json(ApiResponse<DisponibilidadDto>.Fail("Fechas inválidas. Formato esperado: yyyy-MM-dd"), statusCode: 400);
                }
                
                if (dateHasta <= dateDesde)
                {
                    return Results.Json(ApiResponse<DisponibilidadDto>.Fail("La fecha de check-out (fechaHasta) debe ser posterior a la de check-in (fechaDesde)."), statusCode: 400);
                }
                
                var totalNoches = dateHasta.DayNumber - dateDesde.DayNumber;
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                
                var accommodationResponse = await alojamientosClient.GetAsync($"api/v1/Alojamientos/{id}");
                if (accommodationResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Results.Json(ApiResponse<DisponibilidadDto>.Fail("Alojamiento no encontrado."), statusCode: 404);
                }
                if (!accommodationResponse.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<DisponibilidadDto>.Fail($"Error al verificar alojamiento: {accommodationResponse.ReasonPhrase}"), statusCode: (int)accommodationResponse.StatusCode);
                }
                
                var roomsResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/alojamiento/{id}");
                if (!roomsResponse.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<DisponibilidadDto>.Fail("Error al obtener habitaciones del alojamiento."), statusCode: 500);
                }
                
                var rooms = await roomsResponse.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                if (rooms == null || rooms.Count == 0)
                {
                    return Results.Ok(ApiResponse<DisponibilidadDto>.Ok(new DisponibilidadDto
                    {
                        AlojamientoId = id,
                        FechaDesde = dateDesde,
                        FechaHasta = dateHasta,
                        TotalNoches = totalNoches,
                        HabitacionesDisponibles = new()
                    }));
                }
                
                var capacityFilteredRooms = rooms.Where(r => r.CapacidadAdultos >= adultos && r.CapacidadNinos >= ninos).ToList();
                
                var checkOutMinusOne = dateHasta.AddDays(-1);
                var nights = new List<DateOnly>();
                for (var d = dateDesde; d <= checkOutMinusOne; d = d.AddDays(1))
                {
                    nights.Add(d);
                }
                
                var monthYearGroups = nights.GroupBy(n => new { n.Year, n.Month }).ToList();
                var availableRooms = new List<HabitacionDisponibleDto>();
                
                foreach (var room in capacityFilteredRooms)
                {
                    bool isAvailable = true;
                    
                    foreach (var group in monthYearGroups)
                    {
                        var calendarResponse = await alojamientosClient.GetAsync($"api/v1/Calendario/habitacion/{room.HabitacionId}?mes={group.Key.Month}&anio={group.Key.Year}");
                        if (!calendarResponse.IsSuccessStatusCode)
                        {
                            isAvailable = false;
                            break;
                        }
                        
                        var calendarDays = await calendarResponse.Content.ReadFromJsonAsync<List<CalendarioInternalResponse>>();
                        if (calendarDays != null)
                        {
                            var occupiedOrBlocked = calendarDays.Any(c => 
                                nights.Contains(c.Fecha) && 
                                (c.Estado.Equals("Ocupado", StringComparison.OrdinalIgnoreCase) || 
                                 c.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase)));
                                 
                            if (occupiedOrBlocked)
                            {
                                isAvailable = false;
                                break;
                            }
                        }
                    }
                    
                    if (isAvailable)
                    {
                        availableRooms.Add(new HabitacionDisponibleDto
                        {
                            HabitacionId = room.HabitacionId,
                            Nombre = room.Nombre,
                            Descripcion = room.Descripcion,
                            PrecioNoche = room.PrecioNoche,
                            PrecioTotal = room.PrecioNoche * totalNoches,
                            Moneda = "USD",
                            CapacidadAdultos = room.CapacidadAdultos,
                            CapacidadNinos = room.CapacidadNinos,
                            NumDormitorios = room.NumDormitorios,
                            NumBanos = room.NumBanos,
                            TieneCocina = room.TieneCocina,
                            TieneAireAcondicionado = room.TieneAireAcondicionado,
                            SuperficieM2 = room.SuperficieM2
                        });
                    }
                }
                
                var result = new DisponibilidadDto
                {
                    AlojamientoId = id,
                    FechaDesde = dateDesde,
                    FechaHasta = dateHasta,
                    TotalNoches = totalNoches,
                    HabitacionesDisponibles = availableRooms
                };
                
                return Results.Ok(ApiResponse<DisponibilidadDto>.Ok(result));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<DisponibilidadDto>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        // 4. Consultar habitaciones disponibles por fechas (Legacy Route)
        app.MapGet("/api/alojamientos/{id:int}/disponibilidad", async (
            int id,
            IHttpClientFactory httpClientFactory,
            [FromQuery] string fechaDesde,
            [FromQuery] string fechaHasta,
            [FromQuery] int adultos = 1,
            [FromQuery] int ninos = 0) =>
        {
            return await checkDisponibilidad(id, fechaDesde, fechaHasta, adultos, ninos, httpClientFactory);
        })
        .WithName("GetDisponibilidad")
        .WithTags("Disponibilidad")
        .WithOpenApi();

        // 4. Consultar habitaciones disponibles por fechas (V1 Route)
        app.MapGet("/api/v1/alojamientos/{id:int}/disponibilidad", async (
            int id,
            IHttpClientFactory httpClientFactory,
            [FromQuery] string fechaDesde,
            [FromQuery] string fechaHasta,
            [FromQuery] int adultos = 1,
            [FromQuery] int ninos = 0) =>
        {
            return await checkDisponibilidad(id, fechaDesde, fechaHasta, adultos, ninos, httpClientFactory);
        })
        .WithName("GetDisponibilidadV1")
        .WithTags("Disponibilidad")
        .WithOpenApi();

        // 4b. Consultar habitaciones disponibles por fechas (New V2 Route)
        app.MapGet("/api/v2/alojamientos-alojaexpress/{id:int}/disponibilidad", async (
            int id,
            IHttpClientFactory httpClientFactory,
            [FromQuery] string fechaDesde,
            [FromQuery] string fechaHasta,
            [FromQuery] int adultos = 1,
            [FromQuery] int ninos = 0) =>
        {
            return await checkDisponibilidad(id, fechaDesde, fechaHasta, adultos, ninos, httpClientFactory);
        })
        .WithName("GetDisponibilidad_V2")
        .WithTags("Disponibilidad")
        .WithOpenApi();

        // 4c. Consultar disponibilidad con Query parameters (Soporte calendario-alojaexpress)
        app.MapGet("/api/v2/calendario-alojaexpress/disponibilidad", async (
            [FromQuery] int alojamientoId,
            [FromQuery] string fechaInicio,
            [FromQuery] string fechaFin,
            IHttpClientFactory httpClientFactory,
            [FromQuery] int adultos = 1,
            [FromQuery] int ninos = 0) =>
        {
            return await checkDisponibilidad(alojamientoId, fechaInicio, fechaFin, adultos, ninos, httpClientFactory);
        })
        .WithName("GetDisponibilidadCalendario_V2")
        .WithTags("Disponibilidad")
        .WithOpenApi();

        // Obtener habitaciones de una propiedad
        var getHabitacionesHandler = async (
            int propiedadId,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.GetAsync($"api/v1/Habitaciones/alojamiento/{propiedadId}");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail("Error al obtener habitaciones del microservicio."), statusCode: (int)response.StatusCode);
                }

                var rawList = await response.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                if (rawList == null) return Results.Ok(ApiResponse<object>.Ok(new List<object>()));

                var mapped = rawList.Select(h => new
                {
                    habitacionId = h.HabitacionId,
                    alojamientoId = h.AlojamientoId,
                    nombre = h.Nombre,
                    descripcion = h.Descripcion,
                    precioNoche = h.PrecioNoche,
                    capacidadAdultos = h.CapacidadAdultos,
                    capacidadNinos = h.CapacidadNinos,
                    numDormitorios = h.NumDormitorios,
                    numBanos = h.NumBanos,
                    tieneCocina = h.TieneCocina,
                    tieneAireAcondicionado = h.TieneAireAcondicionado,
                    superficieM2 = h.SuperficieM2,
                    estado = h.Estado ?? "Activo",
                    fotos = h.Fotos
                }).ToList();

                return Results.Ok(ApiResponse<object>.Ok(mapped));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/v1/habitaciones/propiedad/{propiedadId:int}", getHabitacionesHandler);
        app.MapGet("/api/v1/habitaciones/por-propiedad/{propiedadId:int}", getHabitacionesHandler);
        app.MapGet("/api/v2/habitaciones-alojaexpress/alojamiento/{propiedadId:int}", getHabitacionesHandler);

        // Crear Habitación
        var crearHabitacionHandler = async (
            JsonElement payload,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var jsonText = payload.GetRawText();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonText, options) ?? new();
                
                if (dict.TryGetValue("propiedadId", out var propIdVal) && !dict.ContainsKey("alojamientoId"))
                {
                    dict["alojamientoId"] = propIdVal;
                }

                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.PostAsJsonAsync("api/v1/Habitaciones", dict);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al registrar habitación: {err}"), statusCode: (int)response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Results.Json(ApiResponse<JsonElement>.Ok(result, "Habitación creada con éxito."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapPost("/api/v1/habitaciones", crearHabitacionHandler)
        .WithName("CrearHabitacion")
        .WithTags("Habitaciones")
        .WithOpenApi();

        app.MapPost("/api/v2/habitaciones-alojaexpress", crearHabitacionHandler)
        .WithName("CrearHabitacion_V2")
        .WithTags("Habitaciones")
        .WithOpenApi();

        // Actualizar Habitación
        var actualizarHabitacionHandler = async (
            int id,
            JsonElement payload,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.PutAsJsonAsync($"api/v1/Habitaciones/{id}", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al actualizar habitación: {err}"), statusCode: (int)response.StatusCode);
                }

                return Results.Ok(ApiResponse<object>.Ok(null, "Habitación actualizada con éxito."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapPut("/api/v1/habitaciones/{id:int}", actualizarHabitacionHandler);
        app.MapPut("/api/v2/habitaciones-alojaexpress/{id:int}", actualizarHabitacionHandler);

        // Desactivar/Eliminar Habitación
        var eliminarHabitacionHandler = async (
            int id,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.DeleteAsync($"api/v1/Habitaciones/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al desactivar/eliminar habitación: {err}"), statusCode: (int)response.StatusCode);
                }

                return Results.Ok(ApiResponse<object>.Ok(null, "Habitación desactivada con éxito."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapDelete("/api/v1/habitaciones/{id:int}", eliminarHabitacionHandler);
        app.MapDelete("/api/v2/habitaciones-alojaexpress/{id:int}", eliminarHabitacionHandler);
    }
}
