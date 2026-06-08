using Alojamientos.DataManagement.Models;

namespace Alojamientos.DataManagement.Interfaces;

public interface IAlojamientosDataService
{
    Task<IEnumerable<AlojamientoDataModel>> GetAllAsync();
    Task<AlojamientoDataModel?> GetByIdAsync(int id);
    Task<AlojamientoDataModel> CreateAsync(AlojamientoDataModel model);
    Task UpdateAsync(AlojamientoDataModel model);
    Task DeleteAsync(int id);
    Task<(IEnumerable<AlojamientoDataModel> Items, int TotalRecords)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? ciudad = null,
        string? tipo = null,
        int? estrellas = null,
        bool? admiteMascotas = null,
        bool? tienePiscina = null);
}

public interface IHabitacionesDataService
{
    Task<IEnumerable<HabitacionDataModel>> GetByAlojamientoIdAsync(int alojamientoId);
    Task<HabitacionDataModel?> GetByIdAsync(int id);
    Task<HabitacionDataModel> CreateAsync(HabitacionDataModel model);
    Task UpdateAsync(HabitacionDataModel model);
    Task DeleteAsync(int id);
}

public interface IAlojamientoFotosDataService
{
    Task<IEnumerable<AlojamientoFotoDataModel>> GetByAlojamientoIdAsync(int alojamientoId);
    Task<AlojamientoFotoDataModel> CreateAsync(AlojamientoFotoDataModel model);
    Task DeleteAsync(int fotoId);
}
