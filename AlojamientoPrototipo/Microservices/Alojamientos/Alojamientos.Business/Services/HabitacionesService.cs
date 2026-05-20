using Alojamientos.Business.DTOs.Habitaciones;
using Alojamientos.Business.Exceptions;
using Alojamientos.Business.Interfaces;
using Alojamientos.Business.Mappers;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.Business.Services;

public class HabitacionesService : IHabitacionesService
{
    private readonly IHabitacionesDataService _dataService;
    private readonly IAlojamientosDataService _alojamientosDataService;

    public HabitacionesService(IHabitacionesDataService dataService, IAlojamientosDataService alojamientosDataService)
    {
        _dataService = dataService;
        _alojamientosDataService = alojamientosDataService;
    }

    public async Task<IEnumerable<HabitacionResponse>> GetByAlojamientoIdAsync(int alojamientoId)
    {
        var models = await _dataService.GetByAlojamientoIdAsync(alojamientoId);
        return models.Select(AlojamientosBusinessMapper.ToResponse);
    }

    public async Task<HabitacionResponse?> GetByIdAsync(int id)
    {
        var model = await _dataService.GetByIdAsync(id);
        return model != null ? AlojamientosBusinessMapper.ToResponse(model) : null;
    }

    public async Task<HabitacionResponse> CrearAsync(CrearHabitacionRequest request)
    {
        var alojamiento = await _alojamientosDataService.GetByIdAsync(request.AlojamientoId)
            ?? throw new AlojamientoNotFoundException(request.AlojamientoId);

        var model = new HabitacionDataModel
        {
            AlojamientoId = request.AlojamientoId,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            CapacidadAdultos = request.CapacidadAdultos,
            CapacidadNinos = request.CapacidadNinos,
            NumBanos = request.NumBanos,
            NumDormitorios = request.NumDormitorios,
            TieneCocina = request.TieneCocina,
            TieneAireAcondicionado = request.TieneAireAcondicionado,
            SuperficieM2 = request.SuperficieM2,
            PrecioNoche = request.PrecioNoche
        };

        var created = await _dataService.CreateAsync(model);
        return AlojamientosBusinessMapper.ToResponse(created);
    }

    public async Task ActualizarAsync(int id, ActualizarHabitacionRequest request)
    {
        var existing = await _dataService.GetByIdAsync(id)
            ?? throw new HabitacionNotFoundException(id);

        existing.Nombre = request.Nombre;
        existing.Descripcion = request.Descripcion;
        existing.CapacidadAdultos = request.CapacidadAdultos;
        existing.CapacidadNinos = request.CapacidadNinos;
        existing.NumBanos = request.NumBanos;
        existing.NumDormitorios = request.NumDormitorios;
        existing.TieneCocina = request.TieneCocina;
        existing.TieneAireAcondicionado = request.TieneAireAcondicionado;
        existing.SuperficieM2 = request.SuperficieM2;
        existing.PrecioNoche = request.PrecioNoche;

        await _dataService.UpdateAsync(existing);
    }

    public async Task EliminarAsync(int id)
    {
        var existing = await _dataService.GetByIdAsync(id)
            ?? throw new HabitacionNotFoundException(id);

        await _dataService.DeleteAsync(id);
    }
}
