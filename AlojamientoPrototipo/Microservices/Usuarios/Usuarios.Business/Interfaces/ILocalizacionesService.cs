using Usuarios.Business.DTOs;

namespace Usuarios.Business.Interfaces;

public interface ILocalizacionesService
{
    Task<IEnumerable<LocalizacionResponse>> GetAllAsync();
    Task<LocalizacionResponse> GetByIdAsync(int id);
    Task<LocalizacionResponse> CrearAsync(CrearLocalizacionRequest request);
    Task EliminarAsync(int id);
}
