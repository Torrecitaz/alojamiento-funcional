using Alojamientos.Business.DTOs.Alojamientos;
using Alojamientos.Business.Exceptions;
using Alojamientos.Business.Interfaces;
using Alojamientos.Business.Mappers;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.Business.Services;

public class AlojamientosService : IAlojamientosService
{
    private readonly IAlojamientosDataService _dataService;

    public AlojamientosService(IAlojamientosDataService dataService) => _dataService = dataService;

    public async Task<IEnumerable<AlojamientoResponse>> GetAllAsync()
    {
        var models = await _dataService.GetAllAsync();
        return models.Select(AlojamientosBusinessMapper.ToResponse);
    }

    public async Task<AlojamientoResponse?> GetByIdAsync(int id)
    {
        var model = await _dataService.GetByIdAsync(id);
        return model != null ? AlojamientosBusinessMapper.ToResponse(model) : null;
    }

    public async Task<AlojamientoResponse> CrearAsync(CrearAlojamientoRequest request)
    {
        var model = new AlojamientoDataModel
        {
            SocioId = request.SocioId,
            TipoAlojamientoId = request.TipoAlojamientoId,
            Nombre = request.Nombre,
            Ciudad = request.Ciudad,
            Direccion = request.Direccion,
            Descripcion = request.Descripcion,
            AdmiteMascotas = request.AdmiteMascotas,
            TienePiscina = request.TienePiscina,
            TieneParqueadero = request.TieneParqueadero
        };

        var created = await _dataService.CreateAsync(model);
        return AlojamientosBusinessMapper.ToResponse(created);
    }

    public async Task ActualizarAsync(int id, ActualizarAlojamientoRequest request)
    {
        var existing = await _dataService.GetByIdAsync(id)
            ?? throw new AlojamientoNotFoundException(id);

        existing.Nombre = request.Nombre;
        existing.Ciudad = request.Ciudad;
        existing.Direccion = request.Direccion;
        existing.Descripcion = request.Descripcion;
        existing.TipoAlojamientoId = request.TipoAlojamientoId;
        existing.AdmiteMascotas = request.AdmiteMascotas;
        existing.TienePiscina = request.TienePiscina;
        existing.TieneParqueadero = request.TieneParqueadero;
        if (request.Estrellas.HasValue) existing.Estrellas = request.Estrellas.Value;

        await _dataService.UpdateAsync(existing);
    }

    public async Task EliminarAsync(int id)
    {
        var existing = await _dataService.GetByIdAsync(id)
            ?? throw new AlojamientoNotFoundException(id);

        await _dataService.DeleteAsync(id);
    }
}
