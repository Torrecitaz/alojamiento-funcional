using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;
using ApiGateway.Models.Internal;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AlojamientoMR - Contrato de Integración para Booking",
        Version = "1.0.0",
        Description = "API pública orientada al flujo del usuario final dentro de la plataforma Booking."
    });
});

// Configure Named HttpClients for microservices
builder.Services.AddHttpClient("Usuarios", client =>
{
    var url = builder.Configuration["Microservices:UsuariosUrl"] ?? "http://localhost:5001";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("Alojamientos", client =>
{
    var url = builder.Configuration["Microservices:AlojamientosUrl"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("Reservas", client =>
{
    var url = builder.Configuration["Microservices:ReservasUrl"] ?? "http://localhost:5003";
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("Facturacion", client =>
{
    var url = builder.Configuration["Microservices:FacturacionUrl"] ?? "http://localhost:5004";
    client.BaseAddress = new Uri(url);
});

// Agregar YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Public API Gateway v1");
    });
}

// ════════════════════════════════════════
// MÓDULO 1: ALOJAMIENTOS
// ════════════════════════════════════════

// 1. Buscar alojamientos
app.MapGet("/api/alojamientos", async (
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
        
        var response = await alojamientosClient.GetAsync("api/v1/Alojamientos");
        if (!response.IsSuccessStatusCode)
        {
            return Results.Json(ApiResponse<List<AlojamientoDto>>.Fail($"Error al obtener alojamientos: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
        }
        
        var rawList = await response.Content.ReadFromJsonAsync<List<AlojamientoInternalResponse>>();
        if (rawList == null)
        {
            return Results.Ok(ApiResponse<List<AlojamientoDto>>.Ok(new()));
        }
        
        var filteredList = rawList.AsEnumerable();
        
        if (!string.IsNullOrEmpty(search))
        {
            filteredList = filteredList.Where(a => 
                a.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                (a.Descripcion != null && a.Descripcion.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }
        
        if (!string.IsNullOrEmpty(ciudad))
        {
            filteredList = filteredList.Where(a => 
                a.Ciudad != null && a.Ciudad.Contains(ciudad, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrEmpty(tipo))
        {
            filteredList = filteredList.Where(a => 
                a.TipoAlojamientoNombre.Equals(tipo, StringComparison.OrdinalIgnoreCase));
        }
        
        if (estrellas.HasValue)
        {
            filteredList = filteredList.Where(a => a.Estrellas >= estrellas.Value);
        }
        
        if (admiteMascotas.HasValue)
        {
            filteredList = filteredList.Where(a => a.AdmiteMascotas == admiteMascotas.Value);
        }
        
        if (tienePiscina.HasValue)
        {
            filteredList = filteredList.Where(a => a.TienePiscina == tienePiscina.Value);
        }
        
        var listToPaginate = filteredList.ToList();
        
        var paginatedList = listToPaginate
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
            
        var resultList = new List<AlojamientoDto>();
        foreach (var item in paginatedList)
        {
            decimal precioMin = 0;
            var roomsResponse = await alojamientosClient.GetAsync($"api/v1/Habitaciones/alojamiento/{item.AlojamientoId}");
            if (roomsResponse.IsSuccessStatusCode)
            {
                var rooms = await roomsResponse.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
                if (rooms != null && rooms.Count > 0)
                {
                    precioMin = rooms.Min(r => r.PrecioNoche);
                }
            }
            
            string? imagenUrl = null;
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
})
.WithName("BuscarAlojamientos")
.WithTags("Alojamientos")
.WithOpenApi();

// 2. Detalle completo de un alojamiento
app.MapGet("/api/alojamientos/{id:int}", async (int id, IHttpClientFactory httpClientFactory) =>
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
})
.WithName("GetAlojamientoDetalle")
.WithTags("Alojamientos")
.WithOpenApi();

// 3. Tipos de alojamiento disponibles
app.MapGet("/api/alojamientos/tipos", async (IHttpClientFactory httpClientFactory) =>
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
})
.WithName("GetTiposAlojamiento")
.WithTags("Alojamientos")
.WithOpenApi();

// ════════════════════════════════════════
// MÓDULO 2: DISPONIBILIDAD
// ════════════════════════════════════════

// 4. Consultar habitaciones disponibles por fechas
app.MapGet("/api/alojamientos/{id:int}/disponibilidad", async (
    int id,
    IHttpClientFactory httpClientFactory,
    [FromQuery] string fechaDesde,
    [FromQuery] string fechaHasta,
    [FromQuery] int adultos = 1,
    [FromQuery] int ninos = 0) =>
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
})
.WithName("GetDisponibilidad")
.WithTags("Disponibilidad")
.WithOpenApi();

// ════════════════════════════════════════
// MÓDULO 3: CLIENTES
// ════════════════════════════════════════

// 5. Registrar nuevo huésped
app.MapPost("/api/usuarios/clientes/registrar", async (
    RegistrarClienteRequest request,
    IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var usuariosClient = httpClientFactory.CreateClient("Usuarios");
        var response = await usuariosClient.PostAsJsonAsync("api/v1/Clientes/registrar", request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errContent = await response.Content.ReadAsStringAsync();
            List<string> errorsList = new();
            try
            {
                var parsedErr = JsonSerializer.Deserialize<Dictionary<string, object>>(errContent);
                if (parsedErr != null && parsedErr.TryGetValue("errors", out var errObj))
                {
                    errorsList.Add(errObj.ToString() ?? errContent);
                }
                else
                {
                    errorsList.Add(errContent);
                }
            }
            catch
            {
                errorsList.Add(errContent);
            }
            
            return Results.Json(ApiResponse<object>.Fail("No se pudo registrar al cliente.", errorsList), statusCode: (int)response.StatusCode);
        }
        
        return Results.Json(ApiResponse<object>.Ok(null, "Cliente registrado exitosamente"), statusCode: 201);
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<object>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
    }
})
.WithName("RegistrarCliente")
.WithTags("Clientes")
.WithOpenApi();

// 6. Buscar cliente por cédula
app.MapGet("/api/usuarios/clientes/cedula/{cedula}", async (
    string cedula,
    IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var usuariosClient = httpClientFactory.CreateClient("Usuarios");
        var response = await usuariosClient.GetAsync($"api/v1/Clientes/cedula/{cedula}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.Json(ApiResponse<ClienteDto>.Fail("No existe cliente con esa cédula."), statusCode: 404);
        }
        if (!response.IsSuccessStatusCode)
        {
            return Results.Json(ApiResponse<ClienteDto>.Fail($"Error al obtener cliente: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
        }
        
        var cliente = await response.Content.ReadFromJsonAsync<ClienteInternalResponse>();
        if (cliente == null)
        {
            return Results.Json(ApiResponse<ClienteDto>.Fail("No existe cliente con esa cédula."), statusCode: 404);
        }
        
        var mapped = new ClienteDto
        {
            ClienteId = cliente.ClienteId,
            NombreCompleto = cliente.Usuario?.NombreCompleto ?? string.Empty,
            Email = cliente.Email,
            Cedula = cliente.Cedula,
            Telefono = cliente.Telefono,
            Domicilio = cliente.Domicilio,
            TotalReservas = cliente.TotalReservas,
            FechaCreacion = cliente.FechaCreacion
        };
        
        return Results.Ok(ApiResponse<ClienteDto>.Ok(mapped));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<ClienteDto>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
    }
})
.WithName("GetClienteByCedula")
.WithTags("Clientes")
.WithOpenApi();

// 7. Perfil del huésped
app.MapGet("/api/usuarios/clientes/{id:int}", async (
    int id,
    IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var usuariosClient = httpClientFactory.CreateClient("Usuarios");
        var response = await usuariosClient.GetAsync($"api/v1/Clientes/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.Json(ApiResponse<ClienteDto>.Fail("Cliente no encontrado."), statusCode: 404);
        }
        if (!response.IsSuccessStatusCode)
        {
            return Results.Json(ApiResponse<ClienteDto>.Fail($"Error al obtener cliente: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
        }
        
        var cliente = await response.Content.ReadFromJsonAsync<ClienteInternalResponse>();
        if (cliente == null)
        {
            return Results.Json(ApiResponse<ClienteDto>.Fail("Cliente no encontrado."), statusCode: 404);
        }
        
        var mapped = new ClienteDto
        {
            ClienteId = cliente.ClienteId,
            NombreCompleto = cliente.Usuario?.NombreCompleto ?? string.Empty,
            Email = cliente.Email,
            Cedula = cliente.Cedula,
            Telefono = cliente.Telefono,
            Domicilio = cliente.Domicilio,
            TotalReservas = cliente.TotalReservas,
            FechaCreacion = cliente.FechaCreacion
        };
        
        return Results.Ok(ApiResponse<ClienteDto>.Ok(mapped));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<ClienteDto>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
    }
})
.WithName("GetPerfilCliente")
.WithTags("Clientes")
.WithOpenApi();

// ════════════════════════════════════════
// MÓDULO 4: RESERVAS
// ════════════════════════════════════════

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
            habitaciones = internalHabitaciones
        };
        
        var reservasClient = httpClientFactory.CreateClient("Reservas");
        var response = await reservasClient.PostAsJsonAsync("api/v1/Reservas", internalReq);
        
        if (!response.IsSuccessStatusCode)
        {
            var errContent = await response.Content.ReadAsStringAsync();
            return Results.Json(ApiResponse<ReservaDto>.Fail($"Error al crear reserva: {errContent}"), statusCode: (int)response.StatusCode);
        }
        
        var internalRes = await response.Content.ReadFromJsonAsync<ReservaInternalResponse>();
        if (internalRes == null)
        {
            return Results.Json(ApiResponse<ReservaDto>.Fail("No se pudo obtener la reserva creada del microservicio."), statusCode: 500);
        }
        
        // Block dates in calendar
        var alojamientosClient = httpClientFactory.CreateClient("Alojamientos");
        var fechaFinExclusiva = request.FechaCheckOut.AddDays(-1);
        
        foreach (var hab in request.Habitaciones)
        {
            var blockReq = new
            {
                habitacionId = hab.HabitacionId,
                fechaInicio = request.FechaCheckIn,
                fechaFin = fechaFinExclusiva
            };
            await alojamientosClient.PostAsJsonAsync("api/v1/Calendario/bloquear", blockReq);
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
        
        var mappedDto = new ReservaDto
        {
            ReservaId = internalRes.ReservaId,
            CodigoReserva = internalRes.CodigoReserva,
            AlojamientoId = internalRes.AlojamientoId,
            NombreAlojamiento = nombreAlojamiento,
            FechaCheckIn = internalRes.FechaCheckIn,
            FechaCheckOut = internalRes.FechaCheckOut,
            NumNoches = numNoches,
            NumAdultos = internalRes.NumAdultos,
            NumNinos = internalRes.NumNinos,
            LlevaMascotas = internalRes.LlevaMascotas,
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
        
        var numNoches = internalRes.FechaCheckOut.DayNumber - internalRes.FechaCheckIn.DayNumber;
        
        var mappedDto = new ReservaDto
        {
            ReservaId = internalRes.ReservaId,
            CodigoReserva = internalRes.CodigoReserva,
            AlojamientoId = internalRes.AlojamientoId,
            NombreAlojamiento = nombreAlojamiento,
            FechaCheckIn = internalRes.FechaCheckIn,
            FechaCheckOut = internalRes.FechaCheckOut,
            NumNoches = numNoches,
            NumAdultos = internalRes.NumAdultos,
            NumNinos = internalRes.NumNinos,
            LlevaMascotas = internalRes.LlevaMascotas,
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
                FechaCheckIn = r.FechaCheckIn,
                FechaCheckOut = r.FechaCheckOut,
                NumNoches = numNoches,
                NumAdultos = r.NumAdultos,
                NumNinos = r.NumNinos,
                LlevaMascotas = r.LlevaMascotas,
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

// 11. Cancelar reserva
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
        
        var statusReq = new { estado = "Cancelada" };
        var patchResponse = await reservasClient.PatchAsJsonAsync($"api/v1/Reservas/{id}/estado", statusReq);
        
        if (!patchResponse.IsSuccessStatusCode)
        {
            var errContent = await patchResponse.Content.ReadAsStringAsync();
            return Results.Json(ApiResponse<object>.Fail($"Error al actualizar estado en el microservicio: {errContent}"), statusCode: (int)patchResponse.StatusCode);
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

// ════════════════════════════════════════
// MÓDULO 5: FACTURACIÓN
// ════════════════════════════════════════

// 12. Comprobante de pago de una reserva
app.MapGet("/api/facturas/reserva/{reservaId:int}", async (
    int reservaId,
    IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var facturacionClient = httpClientFactory.CreateClient("Facturacion");
        
        var factResponse = await facturacionClient.GetAsync($"api/v1/Facturas/reserva/{reservaId}");
        if (factResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.Json(ApiResponse<FacturaDto>.Fail("Factura no encontrada para esa reserva."), statusCode: 404);
        }
        if (!factResponse.IsSuccessStatusCode)
        {
            return Results.Json(ApiResponse<FacturaDto>.Fail($"Error al obtener factura: {factResponse.ReasonPhrase}"), statusCode: (int)factResponse.StatusCode);
        }
        
        var invoice = await factResponse.Content.ReadFromJsonAsync<FacturaInternalResponse>();
        if (invoice == null)
        {
            return Results.Json(ApiResponse<FacturaDto>.Fail("Factura no encontrada para esa reserva."), statusCode: 404);
        }
        
        string codigoReserva = string.Empty;
        var reservasClient = httpClientFactory.CreateClient("Reservas");
        var resResponse = await reservasClient.GetAsync($"api/v1/Reservas/{reservaId}");
        if (resResponse.IsSuccessStatusCode)
        {
            var res = await resResponse.Content.ReadFromJsonAsync<ReservaInternalResponse>();
            if (res != null)
            {
                codigoReserva = res.CodigoReserva;
            }
        }
        
        var mappedDto = new FacturaDto
        {
            FacturaId = invoice.FacturaId,
            ReservaId = invoice.ReservaId,
            CodigoReserva = codigoReserva,
            Monto = invoice.Monto,
            Moneda = "USD",
            MetodoPago = invoice.MetodoPagoTipo ?? "CREDITO",
            Estado = invoice.Estado,
            FechaPago = invoice.FechaPago,
            FechaCreacion = invoice.FechaCreacion
        };
        
        return Results.Ok(ApiResponse<FacturaDto>.Ok(mappedDto));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<FacturaDto>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
    }
})
.WithName("GetFacturaByReserva")
.WithTags("Facturación")
.WithOpenApi();

// 13. Métodos de pago disponibles
app.MapGet("/api/facturas/metodos-pago", async (
    IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var facturacionClient = httpClientFactory.CreateClient("Facturacion");
        var response = await facturacionClient.GetAsync("api/v1/MetodosPago");
        if (!response.IsSuccessStatusCode)
        {
            return Results.Json(ApiResponse<List<MetodoPagoDto>>.Fail($"Error al obtener métodos de pago: {response.ReasonPhrase}"), statusCode: (int)response.StatusCode);
        }
        
        var rawList = await response.Content.ReadFromJsonAsync<List<MetodoPagoInternalResponse>>();
        if (rawList == null)
        {
            return Results.Ok(ApiResponse<List<MetodoPagoDto>>.Ok(new()));
        }
        
        var mapped = rawList.Select(m => new MetodoPagoDto
        {
            MetodoPagoId = m.MetodoPagoId,
            Tipo = m.Tipo
        }).ToList();
        
        return Results.Ok(ApiResponse<List<MetodoPagoDto>>.Ok(mapped));
    }
    catch (Exception ex)
    {
        return Results.Json(ApiResponse<List<MetodoPagoDto>>.Fail($"Error interno: {ex.Message}"), statusCode: 500);
    }
})
.WithName("GetMetodosPago")
.WithTags("Facturación")
.WithOpenApi();

app.MapReverseProxy();

app.Run();
