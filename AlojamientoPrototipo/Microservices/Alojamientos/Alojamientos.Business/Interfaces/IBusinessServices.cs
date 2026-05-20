using Alojamientos.Business.DTOs.Alojamientos;
using Alojamientos.Business.DTOs.Fotos;
using Alojamientos.Business.DTOs.Habitaciones;

namespace Alojamientos.Business.Interfaces;

public interface IAlojamientosService
{
    Task<IEnumerable<AlojamientoResponse>> GetAllAsync();
    Task<AlojamientoResponse?> GetByIdAsync(int id);
    Task<AlojamientoResponse> CrearAsync(CrearAlojamientoRequest request);
    Task ActualizarAsync(int id, ActualizarAlojamientoRequest request);
    Task EliminarAsync(int id);
}

public interface IHabitacionesService
{
    Task<IEnumerable<HabitacionResponse>> GetByAlojamientoIdAsync(int alojamientoId);
    Task<HabitacionResponse?> GetByIdAsync(int id);
    Task<HabitacionResponse> CrearAsync(CrearHabitacionRequest request);
    Task ActualizarAsync(int id, ActualizarHabitacionRequest request);
    Task EliminarAsync(int id);
}

public interface IFotosService
{
    Task<IEnumerable<FotoResponse>> GetByAlojamientoIdAsync(int alojamientoId);
    Task<FotoResponse> AgregarAsync(AgregarFotoRequest request);
    Task EliminarAsync(int id);
}
