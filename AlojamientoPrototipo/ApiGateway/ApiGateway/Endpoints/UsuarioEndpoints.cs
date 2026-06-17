using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ApiGateway.Models;
using ApiGateway.Models.Internal;

namespace ApiGateway.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        // 5. Registrar nuevo huésped
        var registrarClienteHandler = async (
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
        };

        app.MapPost("/api/usuarios/clientes/registrar", registrarClienteHandler)
        .WithName("RegistrarCliente")
        .WithTags("Clientes")
        .WithOpenApi();

        app.MapPost("/api/v2/clientes-alojaexpress/registrar", registrarClienteHandler)
        .WithName("RegistrarCliente_V2")
        .WithTags("Clientes")
        .WithOpenApi();

        // 6. Buscar cliente por cédula
        var getClienteByCedulaHandler = async (
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
        };

        app.MapGet("/api/usuarios/clientes/cedula/{cedula}", getClienteByCedulaHandler)
        .WithName("GetClienteByCedula")
        .WithTags("Clientes")
        .WithOpenApi();

        app.MapGet("/api/v2/clientes-alojaexpress/cedula/{cedula}", getClienteByCedulaHandler)
        .WithName("GetClienteByCedula_V2")
        .WithTags("Clientes")
        .WithOpenApi();

        // 7. Perfil del huésped
        var getPerfilClienteHandler = async (
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
        };

        app.MapGet("/api/usuarios/clientes/{id:int}", getPerfilClienteHandler)
        .WithName("GetPerfilCliente")
        .WithTags("Clientes")
        .WithOpenApi();

        app.MapGet("/api/v2/clientes-alojaexpress/{id:int}", getPerfilClienteHandler)
        .WithName("GetPerfilCliente_V2")
        .WithTags("Clientes")
        .WithOpenApi();
    }
}
