using Alojamientos.Business.DTOs.Fotos;
using Alojamientos.Business.Exceptions;
using Alojamientos.Business.Interfaces;
using Alojamientos.Business.Mappers;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.Business.Services;

public class FotosService : IFotosService
{
    private readonly IAlojamientoFotosDataService _dataService;
    private readonly IAlojamientosDataService _alojamientosDataService;

    public FotosService(IAlojamientoFotosDataService dataService, IAlojamientosDataService alojamientosDataService)
    {
        _dataService = dataService;
        _alojamientosDataService = alojamientosDataService;
    }

    public async Task<IEnumerable<FotoResponse>> GetByAlojamientoIdAsync(int alojamientoId)
    {
        var models = await _dataService.GetByAlojamientoIdAsync(alojamientoId);
        return models.Select(AlojamientosBusinessMapper.ToResponse);
    }

    public async Task<FotoResponse> AgregarAsync(AgregarFotoRequest request)
    {
        var alojamiento = await _alojamientosDataService.GetByIdAsync(request.AlojamientoId)
            ?? throw new AlojamientoNotFoundException(request.AlojamientoId);

        var model = new AlojamientoFotoDataModel
        {
            AlojamientoId = request.AlojamientoId,
            Url = request.Url,
            Orden = request.Orden,
            Descripcion = request.Descripcion
        };

        var created = await _dataService.CreateAsync(model);
        return AlojamientosBusinessMapper.ToResponse(created);
    }

    public async Task EliminarAsync(int id)
    {
        // El manejo de si existe la foto ya lo hace el DataService (arroja KeyNotFoundException)
        // Pero podríamos validar aquí para lanzar FotoNotFoundException.
        try
        {
            await _dataService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            throw new FotoNotFoundException(id);
        }
    }
}
