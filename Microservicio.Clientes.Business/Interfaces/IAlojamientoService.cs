using Microservicio.Clientes.Business.DTOs.Alojamientos;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IAlojamientoService
{
    Task<IEnumerable<AlojamientoResponse>> GetAllAsync();
    Task<AlojamientoResponse?> GetByIdAsync(int id);
    Task<AlojamientoResponse?> GetWithHabitacionesAsync(int id);
    Task<IEnumerable<AlojamientoResponse>> GetByCiudadAsync(string ciudad);
    Task<AlojamientoResponse> CrearAsync(CrearAlojamientoRequest request);
    Task<HabitacionResponse> CrearHabitacionAsync(CrearHabitacionRequest request);
    Task<IEnumerable<HabitacionResponse>> GetHabitacionesByAlojamientoAsync(int alojamientoId);
    Task<IEnumerable<DisponibilidadResponse>> GetDisponibilidadAsync(int habitacionId, DateOnly desde, DateOnly hasta);
    Task<IEnumerable<TipoAlojamientoResponse>> GetTiposAlojamientoAsync();
    Task<TipoAlojamientoResponse> CrearTipoAlojamientoAsync(CrearTipoAlojamientoRequest request);
}
