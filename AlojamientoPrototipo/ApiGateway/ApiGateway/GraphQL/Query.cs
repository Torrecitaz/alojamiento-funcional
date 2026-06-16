using ApiGateway.Models.Internal;
using HotChocolate;
using HotChocolate.Types;
using System.Net.Http.Json;

namespace ApiGateway.GraphQL;

public class Query
{
    public async Task<List<AlojamientoInternalResponse>> GetAlojamientos(
        [Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("Alojamientos");
        var response = await client.GetAsync("api/v1/Alojamientos");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<AlojamientoInternalResponse>>();
            return result ?? new();
        }
        return new();
    }

    public async Task<AlojamientoInternalResponse?> GetAlojamiento(
        int id,
        [Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("Alojamientos");
        var response = await client.GetAsync($"api/v1/Alojamientos/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AlojamientoInternalResponse>();
        }
        return null;
    }
}

public class AlojamientoType : ObjectType<AlojamientoInternalResponse>
{
    protected override void Configure(IObjectTypeDescriptor<AlojamientoInternalResponse> descriptor)
    {
        descriptor.Field(t => t.AlojamientoId).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.Nombre).Type<StringType>();
        descriptor.Field(t => t.Ciudad).Type<StringType>();
        descriptor.Field(t => t.Direccion).Type<StringType>();
        descriptor.Field(t => t.Descripcion).Type<StringType>();
        descriptor.Field(t => t.Estrellas).Type<IntType>();
        descriptor.Field(t => t.CalificacionPromedio).Type<DecimalType>();
        descriptor.Field(t => t.TotalResenas).Type<IntType>();
        descriptor.Field(t => t.AdmiteMascotas).Type<BooleanType>();
        descriptor.Field(t => t.TienePiscina).Type<BooleanType>();
        descriptor.Field(t => t.TieneParqueadero).Type<BooleanType>();
        descriptor.Field(t => t.TipoAlojamientoNombre).Type<StringType>();
        descriptor.Field(t => t.Estado).Type<StringType>();

        descriptor.Field("habitaciones")
            .ResolveWith<AlojamientoResolvers>(r => r.GetHabitaciones(default!, default!))
            .Description("List of rooms for this accommodation");

        descriptor.Field("fotos")
            .ResolveWith<AlojamientoResolvers>(r => r.GetFotos(default!, default!))
            .Description("List of photos for this accommodation");
    }
}

public class AlojamientoResolvers
{
    public async Task<List<HabitacionInternalResponse>> GetHabitaciones(
        [Parent] AlojamientoInternalResponse alojamiento,
        [Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("Alojamientos");
        var response = await client.GetAsync($"api/v1/Habitaciones/alojamiento/{alojamiento.AlojamientoId}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>();
            return result ?? new();
        }
        return new();
    }

    public async Task<List<FotoInternalResponse>> GetFotos(
        [Parent] AlojamientoInternalResponse alojamiento,
        [Service] IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("Alojamientos");
        var response = await client.GetAsync($"api/v1/Fotos/alojamiento/{alojamiento.AlojamientoId}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<FotoInternalResponse>>();
            return result ?? new();
        }
        return new();
    }
}
