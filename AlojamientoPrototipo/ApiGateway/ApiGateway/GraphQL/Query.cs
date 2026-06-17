using ApiGateway.Models.Internal;
using HotChocolate;
using HotChocolate.Types;
using System.Net.Http.Json;
using GreenDonut;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        HabitacionesDataLoader dataLoader)
    {
        return await dataLoader.LoadAsync(alojamiento.AlojamientoId);
    }

    public async Task<List<FotoInternalResponse>> GetFotos(
        [Parent] AlojamientoInternalResponse alojamiento,
        FotosDataLoader dataLoader)
    {
        return await dataLoader.LoadAsync(alojamiento.AlojamientoId);
    }
}

public class HabitacionesDataLoader : BatchDataLoader<int, List<HabitacionInternalResponse>>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HabitacionesDataLoader(
        IBatchScheduler batchScheduler,
        IHttpClientFactory httpClientFactory)
        : base(batchScheduler)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<IReadOnlyDictionary<int, List<HabitacionInternalResponse>>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("Alojamientos");
        var tasks = keys.Select(async id =>
        {
            try
            {
                var response = await client.GetAsync($"api/v1/Habitaciones/alojamiento/{id}", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<HabitacionInternalResponse>>(cancellationToken: cancellationToken);
                    return (id, list ?? new());
                }
            }
            catch
            {
                // Ignorar error y retornar lista vacia
            }
            return (id, new List<HabitacionInternalResponse>());
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(x => x.id, x => x.Item2);
    }
}

public class FotosDataLoader : BatchDataLoader<int, List<FotoInternalResponse>>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FotosDataLoader(
        IBatchScheduler batchScheduler,
        IHttpClientFactory httpClientFactory)
        : base(batchScheduler)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<IReadOnlyDictionary<int, List<FotoInternalResponse>>> LoadBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("Alojamientos");
        var tasks = keys.Select(async id =>
        {
            try
            {
                var response = await client.GetAsync($"api/v1/Fotos/alojamiento/{id}", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<FotoInternalResponse>>(cancellationToken: cancellationToken);
                    return (id, list ?? new());
                }
            }
            catch
            {
                // Ignorar error y retornar lista vacia
            }
            return (id, new List<FotoInternalResponse>());
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(x => x.id, x => x.Item2);
    }
}
