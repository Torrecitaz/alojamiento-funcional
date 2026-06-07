using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ApiGateway.Models;
using ApiGateway.Models.Internal;

namespace ApiGateway.Endpoints;

public static class FacturacionEndpoints
{
    public static void MapFacturacionEndpoints(this IEndpointRouteBuilder app)
    {
        // 12. Comprobante de pago de una reserva (Mapped with /api/v1/ prefix)
        app.MapGet("/api/v1/facturas/reserva/{reservaId:int}", async (
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

        // 13. Métodos de pago disponibles (Mapped with /api/v1/ prefix)
        app.MapGet("/api/v1/facturas/metodos-pago", async (
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
    }
}
