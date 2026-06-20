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

public static class PropiedadesEndpoints
{
    public static void MapPropiedadesEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Buscar alojamientos (API pública con DTO unificado)
        var buscarAlojamientosHandler = async (
            IHttpClientFactory httpClientFactory,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? ciudad = null,
            [FromQuery] string? tipo = null,
            [FromQuery] int? estrellas = null,
            [FromQuery] bool? admiteMascotas = null,
            [FromQuery] bool? tienePiscina = null) =>
        {
            try
            {
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(ciudad)) queryParams.Add($"ciudad={Uri.EscapeDataString(ciudad)}");
                if (!string.IsNullOrEmpty(tipo)) queryParams.Add($"tipo={Uri.EscapeDataString(tipo)}");
                if (estrellas.HasValue) queryParams.Add($"estrellas={estrellas.Value}");
                if (admiteMascotas.HasValue) queryParams.Add($"admiteMascotas={admiteMascotas.Value.ToString().ToLower()}");
                if (tienePiscina.HasValue) queryParams.Add($"tienePiscina={tienePiscina.Value.ToString().ToLower()}");

                var queryString = string.Join("&", queryParams);
                var response = await alojamientosClient.GetAsync($"api/v1/Alojamientos/buscar?{queryString}");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<List<AlojamientoDto>>.Fail($"Error al obtener alojamientos: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
                }
                
                var pagedResult = await response.Content.ReadFromJsonAsync<AlojamientoPagedInternalResponse>();
                if (pagedResult == null || pagedResult.Items == null)
                {
                    return Results.Ok(ApiResponse<List<AlojamientoDto>>.Ok(new()));
                }
                
                var paginatedList = pagedResult.Items;
                    
                var resultList = new List<AlojamientoDto>();
                foreach (var item in paginatedList)
                {
                    decimal precioMin = 0;
                    string? imagenUrl = null;

                    var roomsResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/alojamiento/{item.AlojamientoId}");
                    if (roomsResponse.IsSuccessStatusCode)
                    {
                        var rooms = await roomsResponse.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                        if (rooms != null && rooms.Count > 0)
                        {
                            precioMin = rooms.Min(r => r.PrecioNoche);
                        }
                    }

                    var photosResponse = await alojamientosClient.GetAsync($"api/v1/Fotos/alojamiento/{item.AlojamientoId}");
                    if (photosResponse.IsSuccessStatusCode)
                    {
                        var photos = await photosResponse.Content.ReadFromJsonAsync<List<FotoInternalResponse>>();
                        if (photos != null && photos.Count > 0)
                        {
                            imagenUrl = photos.OrderBy(p => p.Orden).First().Url;
                        }
                    }

                    resultList.Add(new AlojamientoDto
                    {
                        AlojamientoId = item.AlojamientoId,
                        Nombre = item.Nombre,
                        TipoAlojamiento = item.TipoAlojamientoNombre,
                        Ciudad = item.Ciudad ?? string.Empty,
                        Direccion = item.Direccion,
                        PrecioNocheMinimo = precioMin,
                        Moneda = "USD",
                        Estrellas = item.Estrellas,
                        ImagenUrl = imagenUrl,
                        AdmiteMascotas = item.AdmiteMascotas,
                        TienePiscina = item.TienePiscina,
                        TieneParqueadero = item.TieneParqueadero,
                        Disponible = true
                    });
                }
                
                return Results.Ok(ApiResponse<List<AlojamientoDto>>.Ok(resultList));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<List<AlojamientoDto>>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/alojamientos", buscarAlojamientosHandler)
        .WithName("BuscarAlojamientos")
        .WithTags("Alojamientos")
        .WithOpenApi();

        app.MapGet("/api/v2/alojamientos-alojaexpress", buscarAlojamientosHandler)
        .WithName("BuscarAlojamientos_V2")
        .WithTags("Alojamientos")
        .WithOpenApi();

        // 2. Detalle completo de un alojamiento
        var getAlojamientoDetalleHandler = async (int id, IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                
                var response = await alojamientosClient.GetAsync($"api/v1/Alojamientos/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Results.Json(ApiResponse<AlojamientoDetalleDto>.Fail("Alojamiento no encontrado."), statusCode: 404);
                }
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<AlojamientoDetalleDto>.Fail($"Error al obtener alojamiento: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
                }
                
                var item = await response.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                if (item == null)
                {
                    return Results.Json(ApiResponse<AlojamientoDetalleDto>.Fail("Alojamiento no encontrado."), statusCode: 404);
                }
                
                decimal precioMin = 0;
                var roomsResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/alojamiento/{id}");
                if (roomsResponse.IsSuccessStatusCode)
                {
                    var rooms = await roomsResponse.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                    if (rooms != null && rooms.Count > 0)
                    {
                        precioMin = rooms.Min(r => r.PrecioNoche);
                    }
                }
                
                var photoList = new List<FotoDto>();
                string? imagenUrl = null;
                var photosResponse = await alojamientosClient.GetAsync($"api/v1/Fotos/alojamiento/{id}");
                if (photosResponse.IsSuccessStatusCode)
                {
                    var photos = await photosResponse.Content.ReadFromJsonAsync<List<FotoInternalResponse>>();
                    if (photos != null && photos.Count > 0)
                    {
                        var sorted = photos.OrderBy(p => p.Orden).ToList();
                        imagenUrl = sorted.First().Url;
                        photoList = sorted.Select(p => new FotoDto
                        {
                            Url = p.Url,
                            Descripcion = p.Descripcion
                        }).ToList();
                    }
                }
                
                var detail = new AlojamientoDetalleDto
                {
                    AlojamientoId = item.AlojamientoId,
                    Nombre = item.Nombre,
                    TipoAlojamiento = item.TipoAlojamientoNombre,
                    Ciudad = item.Ciudad ?? string.Empty,
                    Direccion = item.Direccion,
                    PrecioNocheMinimo = precioMin,
                    Moneda = "USD",
                    Estrellas = item.Estrellas,
                    ImagenUrl = imagenUrl,
                    AdmiteMascotas = item.AdmiteMascotas,
                    TienePiscina = item.TienePiscina,
                    TieneParqueadero = item.TieneParqueadero,
                    Disponible = true,
                    
                    Descripcion = item.Descripcion,
                    CalificacionPromedio = item.CalificacionPromedio,
                    TotalResenas = item.TotalResenas,
                    Fotos = photoList
                };
                
                return Results.Ok(ApiResponse<AlojamientoDetalleDto>.Ok(detail));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<AlojamientoDetalleDto>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/alojamientos/{id:int}", getAlojamientoDetalleHandler)
        .WithName("GetAlojamientoDetalle")
        .WithTags("Alojamientos")
        .WithOpenApi();

        app.MapGet("/api/v2/alojamientos-alojaexpress/{id:int}", getAlojamientoDetalleHandler)
        .WithName("GetAlojamientoDetalle_V2")
        .WithTags("Alojamientos")
        .WithOpenApi();

        // 3. Tipos de alojamiento disponibles
        var getTiposAlojamientoHandler = async (IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                var response = await alojamientosClient.GetAsync("api/v1/Alojamientos/tipos");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<List<TipoAlojamientoDto>>.Fail($"Error al obtener tipos: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
                }
                
                var rawList = await response.Content.ReadFromJsonAsync<List<TipoAlojamientoInternalResponse>>();
                if (rawList == null)
                {
                    return Results.Ok(ApiResponse<List<TipoAlojamientoDto>>.Ok(new()));
                }
                
                var mapped = rawList.Select(t => new TipoAlojamientoDto
                {
                    TipoAlojamientoId = t.TipoAlojamientoId,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion
                }).ToList();
                
                return Results.Ok(ApiResponse<List<TipoAlojamientoDto>>.Ok(mapped));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<List<TipoAlojamientoDto>>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/alojamientos/tipos", getTiposAlojamientoHandler)
        .WithName("GetTiposAlojamiento")
        .WithTags("Alojamientos")
        .WithOpenApi();

        app.MapGet("/api/v2/alojamientos-alojaexpress/tipos", getTiposAlojamientoHandler)
        .WithName("GetTiposAlojamiento_V2")
        .WithTags("Alojamientos")
        .WithOpenApi();

        // 4. Maestros - Ciudades
        var maestrosCiudadesHandler = () =>
        {
            var ciudades = new[]
            {
                new { ciudadId = 1, nombre = "Quito", pais = "Ecuador" },
                new { ciudadId = 2, nombre = "Guayaquil", pais = "Ecuador" },
                new { ciudadId = 3, nombre = "Cuenca", pais = "Ecuador" },
                new { ciudadId = 4, nombre = "Manta", pais = "Ecuador" }
            };
            return Results.Ok(ApiResponse<object>.Ok(ciudades));
        };

        app.MapGet("/api/v1/maestros/ciudades", maestrosCiudadesHandler);
        app.MapGet("/api/v2/alojamientos-alojaexpress/ciudades", maestrosCiudadesHandler);

        // 5. Maestros - Tipos Alojamiento (Raw response)
        var maestrosTiposAlojamientoHandler = async (IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.GetAsync("api/v1/Alojamientos/tipos");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail("Error al cargar tipos de alojamiento."), statusCode: (int)response.StatusCode);
                }
                var data = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
                return Results.Ok(ApiResponse<object>.Ok(data));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/v1/maestros/tipos-alojamiento", maestrosTiposAlojamientoHandler);
        app.MapGet("/api/v2/alojamientos-alojaexpress/tipos-alojamiento", maestrosTiposAlojamientoHandler);

        // 6. Buscar propiedades (Filtros detallados del panel administrativo/cliente)
        var propiedadesBuscarHandler = async (
            IHttpClientFactory httpClientFactory,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? ciudad = null,
            [FromQuery] string? tipo = null,
            [FromQuery] int? estrellas = null,
            [FromQuery] bool? admiteMascotas = null,
            [FromQuery] bool? tienePiscina = null) =>
        {
            try
            {
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(ciudad)) queryParams.Add($"ciudad={Uri.EscapeDataString(ciudad)}");
                if (!string.IsNullOrEmpty(tipo)) queryParams.Add($"tipo={Uri.EscapeDataString(tipo)}");
                if (estrellas.HasValue) queryParams.Add($"estrellas={estrellas.Value}");
                if (admiteMascotas.HasValue) queryParams.Add($"admiteMascotas={admiteMascotas.Value.ToString().ToLower()}");
                if (tienePiscina.HasValue) queryParams.Add($"tienePiscina={tienePiscina.Value.ToString().ToLower()}");

                var queryString = string.Join("&", queryParams);
                var response = await alojamientosClient.GetAsync($"api/v1/Alojamientos/buscar?{queryString}");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail($"Error al obtener alojamientos: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
                }
                
                var pagedResult = await response.Content.ReadFromJsonAsync<AlojamientoPagedInternalResponse>();
                if (pagedResult == null || pagedResult.Items == null)
                {
                    return Results.Ok(ApiResponse<object>.Ok(new { items = new List<AlojamientoDto>(), totalRecords = 0 }));
                }
                
                var paginatedList = pagedResult.Items;
                    
                var resultList = new List<AlojamientoDto>();
                foreach (var item in paginatedList)
                {
                    decimal precioMin = 0;
                    string? imagenUrl = null;

                    var roomsResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/alojamiento/{item.AlojamientoId}");
                    if (roomsResponse.IsSuccessStatusCode)
                    {
                        var rooms = await roomsResponse.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                        if (rooms != null && rooms.Count > 0)
                        {
                            precioMin = rooms.Where(r => r.Estado != "Inactivo" && r.Estado != "Inactiva").Min(r => (decimal?)r.PrecioNoche) ?? 0;
                        }
                    }

                    var photosResponse = await alojamientosClient.GetAsync($"api/v1/Fotos/alojamiento/{item.AlojamientoId}");
                    if (photosResponse.IsSuccessStatusCode)
                    {
                        var photos = await photosResponse.Content.ReadFromJsonAsync<List<FotoInternalResponse>>();
                        if (photos != null && photos.Count > 0)
                        {
                            imagenUrl = photos.OrderBy(p => p.Orden).First().Url;
                        }
                    }

                    resultList.Add(new AlojamientoDto
                    {
                        AlojamientoId = item.AlojamientoId,
                        Nombre = item.Nombre,
                        TipoAlojamiento = item.TipoAlojamientoNombre,
                        Ciudad = item.Ciudad ?? string.Empty,
                        Direccion = item.Direccion,
                        PrecioNocheMinimo = precioMin,
                        Moneda = "USD",
                        Estrellas = item.Estrellas,
                        ImagenUrl = imagenUrl,
                        AdmiteMascotas = item.AdmiteMascotas,
                        TienePiscina = item.TienePiscina,
                        TieneParqueadero = item.TieneParqueadero,
                        Disponible = true
                    });
                }
                
                return Results.Ok(ApiResponse<object>.Ok(new
                {
                    items = resultList,
                    totalRecords = pagedResult.TotalRecords
                }));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/v1/propiedades/buscar", propiedadesBuscarHandler);
        app.MapGet("/api/v2/alojamientos-alojaexpress/buscar", propiedadesBuscarHandler);

        // 7. Propiedades por Colaborador
        var propiedadesColaboradorHandler = async (
            int colaboradorId,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                var response = await alojamientosClient.GetAsync("api/v1/Alojamientos");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail("Error al obtener alojamientos del microservicio."), statusCode: (int)response.StatusCode);
                }
                
                var rawList = await response.Content.ReadFromJsonAsync<List<AlojamientoInternalResponse>>();
                if (rawList == null)
                {
                    return Results.Ok(ApiResponse<object>.Ok(new List<AlojamientoDto>()));
                }
                
                var filtered = rawList.Where(a => a.SocioId == colaboradorId).ToList();
                
                var resultList = new List<object>();
                foreach (var item in filtered)
                {
                    decimal precioMin = 0;
                    string? imagenUrl = null;

                    var roomsResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/alojamiento/{item.AlojamientoId}");
                    if (roomsResponse.IsSuccessStatusCode)
                    {
                        var rooms = await roomsResponse.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                        if (rooms != null && rooms.Count > 0)
                        {
                            precioMin = rooms.Where(r => r.Estado != "Inactivo" && r.Estado != "Inactiva").Min(r => (decimal?)r.PrecioNoche) ?? 0;
                        }
                    }

                    var photosResponse = await alojamientosClient.GetAsync($"api/v1/Fotos/alojamiento/{item.AlojamientoId}");
                    if (photosResponse.IsSuccessStatusCode)
                    {
                        var photos = await photosResponse.Content.ReadFromJsonAsync<List<FotoInternalResponse>>();
                        if (photos != null && photos.Count > 0)
                        {
                            imagenUrl = photos.OrderBy(p => p.Orden).First().Url;
                        }
                    }

                    resultList.Add(new
                    {
                        propiedadId = item.AlojamientoId,
                        nombre = item.Nombre,
                        tipoAlojamiento = item.TipoAlojamientoNombre,
                        ciudad = item.Ciudad ?? string.Empty,
                        direccion = item.Direccion,
                        estrellas = item.Estrellas,
                        estado = item.Estado == "Activo" || item.Estado == "Activa" ? "Activa" : "Inactiva"
                    });
                }
                return Results.Ok(ApiResponse<object>.Ok(resultList));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/v1/propiedades/colaborador/{colaboradorId:int}", propiedadesColaboradorHandler);
        app.MapGet("/api/v2/alojamientos-alojaexpress/colaborador/{colaboradorId:int}", propiedadesColaboradorHandler);

        // 8. Obtener Propiedad por ID (Formato extendido administrador)
        var propiedadesGetByIdHandler = async (
            int id,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
                var response = await alojamientosClient.GetAsync($"api/v1/Alojamientos/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Results.Json(ApiResponse<object>.Fail("Propiedad no encontrada."), statusCode: 404);
                }
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail("Error al obtener la propiedad."), statusCode: (int)response.StatusCode);
                }

                var item = await response.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                if (item == null)
                {
                    return Results.Json(ApiResponse<object>.Fail("Propiedad no encontrada."), statusCode: 404);
                }

                // Obtener fotos
                var photosResponse = await alojamientosClient.GetAsync($"api/v1/Fotos/alojamiento/{id}");
                var fotos = new List<FotoDto>();
                if (photosResponse.IsSuccessStatusCode)
                {
                    var rawPhotos = await photosResponse.Content.ReadFromJsonAsync<List<FotoInternalResponse>>();
                    if (rawPhotos != null)
                    {
                        fotos = rawPhotos.Select(p => new FotoDto { Url = p.Url, Descripcion = p.Descripcion }).ToList();
                    }
                }

                var detail = new
                {
                    propiedadId = item.AlojamientoId,
                    nombre = item.Nombre,
                    tipoAlojamiento = item.TipoAlojamientoNombre,
                    ciudad = item.Ciudad ?? string.Empty,
                    direccion = item.Direccion,
                    descripcion = item.Descripcion,
                    estrellas = item.Estrellas,
                    admiteMascotas = item.AdmiteMascotas,
                    tienePiscina = item.TienePiscina,
                    tieneParqueadero = item.TieneParqueadero,
                    provincia = item.Provincia,
                    pais = item.Pais,
                    politicas = item.Politicas,
                    checkInTime = item.CheckInTime,
                    checkOutTime = item.CheckOutTime,
                    servicios = item.Servicios,
                    latitud = item.Latitud,
                    longitud = item.Longitud,
                    fotos = fotos
                };

                return Results.Ok(ApiResponse<object>.Ok(detail));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapGet("/api/v1/propiedades/{id:int}", propiedadesGetByIdHandler);
        app.MapGet("/api/v2/alojamientos-alojaexpress/por-id/{id:int}", propiedadesGetByIdHandler);

        // 9. Crear Propiedad
        var crearPropiedadHandler = async (
            JsonElement payload,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var jsonText = payload.GetRawText();
                var req = JsonSerializer.Deserialize<CrearPropiedadFrontendRequest>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (req == null) return Results.Json(ApiResponse<object>.Fail("Payload inválido."), statusCode: 400);

                string GetCiudadNombre(int ciudadId) => ciudadId switch
                {
                    1 => "Quito",
                    2 => "Guayaquil",
                    3 => "Cuenca",
                    4 => "Manta",
                    _ => "Quito"
                };

                var backReq = new
                {
                    socioId = req.ColaboradorId,
                    tipoAlojamientoId = req.TipoAlojamientoId,
                    nombre = req.Nombre,
                    ciudad = GetCiudadNombre(req.CiudadId),
                    direccion = req.Direccion,
                    descripcion = req.Descripcion,
                    admiteMascotas = req.AdmiteMascotas,
                    tienePiscina = false,
                    tieneParqueadero = false,
                    provincia = req.Provincia ?? "Pichincha",
                    pais = req.Pais ?? "Ecuador",
                    politicas = req.Politicas,
                    checkInTime = req.CheckInTime ?? "14:00",
                    checkOutTime = req.CheckOutTime ?? "11:00",
                    servicios = req.Servicios,
                    latitud = req.Latitud,
                    longitud = req.Longitud
                };

                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.PostAsJsonAsync("api/v1/Alojamientos", backReq);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al crear propiedad en el microservicio: {err}"), statusCode: (int)response.StatusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Results.Json(ApiResponse<JsonElement>.Ok(result, "Propiedad creada con éxito."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapPost("/api/v1/propiedades", crearPropiedadHandler)
        .WithName("CrearPropiedad")
        .WithTags("Alojamientos")
        .WithOpenApi();

        app.MapPost("/api/v2/alojamientos-alojaexpress", crearPropiedadHandler)
        .WithName("CrearPropiedad_V2")
        .WithTags("Alojamientos")
        .WithOpenApi();

        // 10. Actualizar Propiedad
        var actualizarPropiedadHandler = async (
            int id,
            JsonElement payload,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var client = httpClientFactory.CreateClient("Alojamientos");
                var response = await client.PutAsJsonAsync($"api/v1/Alojamientos/{id}", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al actualizar propiedad: {err}"), statusCode: (int)response.StatusCode);
                }

                return Results.Ok(ApiResponse<object>.Ok(null, "Propiedad actualizada con éxito."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapPut("/api/v1/propiedades/{id:int}", actualizarPropiedadHandler);
        app.MapPut("/api/v2/alojamientos-alojaexpress/{id:int}", actualizarPropiedadHandler);

        // 11. Duplicar Propiedad
        var duplicarPropiedadHandler = async (
            int id,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var client = httpClientFactory.CreateClient("Alojamientos");
                
                // 1. Obtener alojamiento original
                var getRes = await client.GetAsync($"api/v1/Alojamientos/{id}");
                if (!getRes.IsSuccessStatusCode)
                {
                    return Results.Json(ApiResponse<object>.Fail("Alojamiento original no encontrado."), statusCode: 404);
                }
                
                var existing = await getRes.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                if (existing == null)
                {
                    return Results.Json(ApiResponse<object>.Fail("No se pudo deserializar el alojamiento original."), statusCode: 404);
                }
                
                // 2. Crear payload de copia
                var backReq = new
                {
                    socioId = existing.SocioId,
                    tipoAlojamientoId = existing.TipoAlojamientoId,
                    nombre = existing.Nombre + " - Copia",
                    ciudad = existing.Ciudad,
                    direccion = existing.Direccion,
                    descripcion = existing.Descripcion,
                    admiteMascotas = existing.AdmiteMascotas,
                    tienePiscina = existing.TienePiscina,
                    tieneParqueadero = existing.TieneParqueadero,
                    provincia = existing.Provincia ?? "Pichincha",
                    pais = existing.Pais ?? "Ecuador",
                    politicas = existing.Politicas,
                    checkInTime = existing.CheckInTime ?? "14:00",
                    checkOutTime = existing.CheckOutTime ?? "11:00",
                    servicios = existing.Servicios,
                    latitud = existing.Latitud,
                    longitud = existing.Longitud,
                    estado = "Pendiente"
                };
                
                // 3. Crear nuevo alojamiento
                var createRes = await client.PostAsJsonAsync("api/v1/Alojamientos", backReq);
                if (!createRes.IsSuccessStatusCode)
                {
                    var err = await createRes.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al duplicar alojamiento: {err}"), statusCode: (int)createRes.StatusCode);
                }
                
                var newAlojamiento = await createRes.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                if (newAlojamiento == null)
                {
                    return Results.Json(ApiResponse<object>.Fail("Error al leer alojamiento duplicado."), statusCode: 500);
                }
                
                // 4. Obtener habitaciones originales
                var roomsRes = await client.GetAsync($"api/v1/Habitaciones/alojamiento/{id}");
                if (roomsRes.IsSuccessStatusCode)
                {
                    var rooms = await roomsRes.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                    if (rooms != null)
                    {
                        foreach (var room in rooms)
                        {
                            var newRoomReq = new
                            {
                                alojamientoId = newAlojamiento.AlojamientoId,
                                nombre = room.Nombre,
                                descripcion = room.Descripcion,
                                capacidadAdultos = room.CapacidadAdultos,
                                capacidadNinos = room.CapacidadNinos,
                                numBanos = room.NumBanos,
                                numDormitorios = room.NumDormitorios,
                                tieneCocina = room.TieneCocina,
                                tieneAireAcondicionado = room.TieneAireAcondicionado,
                                superficieM2 = room.SuperficieM2,
                                precioNoche = room.PrecioNoche,
                                estado = room.Estado,
                                fotos = room.Fotos
                            };
                            
                            await client.PostAsJsonAsync("api/v1/Habitaciones", newRoomReq);
                        }
                    }
                }
                
                return Results.Json(ApiResponse<AlojamientoInternalResponse>.Ok(newAlojamiento, "Alojamiento y habitaciones duplicados con éxito."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapPost("/api/v1/propiedades/duplicar/{id:int}", duplicarPropiedadHandler);
        app.MapPost("/api/v2/alojamientos-alojaexpress/duplicar/{id:int}", duplicarPropiedadHandler);

        // 12. Actualizar Estado Propiedad
        var estadoPropiedadHandler = async (
            int id,
            JsonElement payload,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                var root = payload.Clone();
                string nuevoEstado = root.GetProperty("nuevoEstado").GetString() ?? "Activa";

                var client = httpClientFactory.CreateClient("Alojamientos");
                
                if (nuevoEstado.Equals("Inactiva", StringComparison.OrdinalIgnoreCase) || nuevoEstado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
                {
                    var deleteResponse = await client.DeleteAsync($"api/v1/Alojamientos/{id}");
                    if (!deleteResponse.IsSuccessStatusCode)
                    {
                        var err = await deleteResponse.Content.ReadAsStringAsync();
                        return Results.Json(ApiResponse<object>.Fail($"Error al desactivar propiedad: {err}"), statusCode: (int)deleteResponse.StatusCode);
                    }
                }
                else
                {
                    var getRes = await client.GetAsync($"api/v1/Alojamientos/{id}");
                    if (!getRes.IsSuccessStatusCode) return Results.Json(ApiResponse<object>.Fail("No se pudo obtener la propiedad."), statusCode: 404);
                    
                    var existing = await getRes.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
                    if (existing != null)
                    {
                        var backReq = new
                        {
                            nombre = existing.Nombre,
                            ciudad = existing.Ciudad,
                            direccion = existing.Direccion,
                            descripcion = existing.Descripcion,
                            tipoAlojamientoId = existing.TipoAlojamientoId,
                            admiteMascotas = existing.AdmiteMascotas,
                            tienePiscina = existing.TienePiscina,
                            tieneParqueadero = existing.TieneParqueadero,
                            estrellas = existing.Estrellas,
                            provincia = existing.Provincia,
                            pais = existing.Pais,
                            politicas = existing.Politicas,
                            checkInTime = existing.CheckInTime,
                            checkOutTime = existing.CheckOutTime,
                            servicios = existing.Servicios,
                            latitud = existing.Latitud,
                            longitud = existing.Longitud,
                            estado = "Activo"
                        };
                        
                        var putResponse = await client.PutAsJsonAsync($"api/v1/Alojamientos/{id}", backReq);
                        if (!putResponse.IsSuccessStatusCode)
                        {
                            var err = await putResponse.Content.ReadAsStringAsync();
                            return Results.Json(ApiResponse<object>.Fail($"Error al activar propiedad: {err}"), statusCode: (int)putResponse.StatusCode);
                        }
                    }
                }

                return Results.Ok(ApiResponse<object>.Ok(null, "Estado actualizado con éxito."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapPatch("/api/v1/propiedades/{id:int}/estado", estadoPropiedadHandler);
        app.MapPatch("/api/v2/alojamientos-alojaexpress/{id:int}/estado", estadoPropiedadHandler);

        // 13. Subir Fotos Propiedad
        var fotosPropiedadHandler = async (
            int id,
            HttpRequest request,
            Shared.Kernel.Services.ICloudinaryService cloudinaryService,
            IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                if (!request.HasFormContentType)
                {
                    return Results.Json(ApiResponse<object>.Fail("Petición inválida: Tipo de contenido debe ser multipart/form-data."), statusCode: 400);
                }

                var form = await request.ReadFormAsync();
                var file = form.Files.GetFile("file");
                if (file == null || file.Length == 0)
                {
                    return Results.Json(ApiResponse<object>.Fail("No se encontró ningún archivo en la petición."), statusCode: 400);
                }

                // 1. Subir la imagen a Cloudinary
                using var stream = file.OpenReadStream();
                var imageUrl = await cloudinaryService.UploadImageAsync(file.FileName, stream);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return Results.Json(ApiResponse<object>.Fail("Error al procesar la imagen en el servidor de almacenamiento."), statusCode: 500);
                }

                // 2. Registrar la foto en el microservicio de Alojamientos
                var client = httpClientFactory.CreateClient("Alojamientos");
                var backReq = new
                {
                    alojamientoId = id,
                    url = imageUrl,
                    orden = 0,
                    descripcion = file.FileName
                };

                var response = await client.PostAsJsonAsync("api/v1/Fotos", backReq);
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Json(ApiResponse<object>.Fail($"Error al registrar foto en Alojamientos: {err}"), statusCode: (int)response.StatusCode);
                }

                var photoResult = await response.Content.ReadFromJsonAsync<JsonElement>();
                return Results.Json(ApiResponse<JsonElement>.Ok(photoResult, "Foto subida y asociada correctamente."));
            }
            catch (Exception ex)
            {
                return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
            }
        };

        app.MapPost("/api/v1/propiedades/{id:int}/fotos", fotosPropiedadHandler);
        app.MapPost("/api/v2/alojamientos-alojaexpress/{id:int}/fotos", fotosPropiedadHandler);
        app.MapPost("/api/v2/fotos-alojaexpress/alojamiento/{id:int}", fotosPropiedadHandler);
    }
}
