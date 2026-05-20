using Alojamientos.DataAccess.Entities;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Mappers;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.DataManagement.Services;

public class HabitacionesDataService : IHabitacionesDataService
{
    private readonly IHabitacionesRepository _repo;

    public HabitacionesDataService(IHabitacionesRepository repo) => _repo = repo;

    public async Task<IEnumerable<HabitacionDataModel>> GetByAlojamientoIdAsync(int alojamientoId)
    {
        var entities = await _repo.FindAsync(h => h.AlojamientoId == alojamientoId);
        return entities.Select(AlojamientosMapper.ToDataModel);
    }

    public async Task<HabitacionDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? AlojamientosMapper.ToDataModel(entity) : null;
    }

    public async Task<HabitacionDataModel> CreateAsync(HabitacionDataModel model)
    {
        var entity = new HabitacionEntity
        {
            AlojamientoId = model.AlojamientoId,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            CapacidadAdultos = model.CapacidadAdultos,
            CapacidadNinos = model.CapacidadNinos,
            NumBanos = model.NumBanos,
            NumDormitorios = model.NumDormitorios,
            TieneCocina = model.TieneCocina,
            TieneAireAcondicionado = model.TieneAireAcondicionado,
            SuperficieM2 = model.SuperficieM2,
            PrecioNoche = model.PrecioNoche
        };
        var created = await _repo.AddAsync(entity);
        return AlojamientosMapper.ToDataModel(created);
    }

    public async Task UpdateAsync(HabitacionDataModel model)
    {
        var entity = await _repo.GetByIdAsync(model.HabitacionId);
        if (entity == null) throw new KeyNotFoundException($"Habitación {model.HabitacionId} no encontrada");

        AlojamientosMapper.UpdateEntity(entity, model);
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Habitación {id} no encontrada");
        await _repo.DeleteAsync(entity);
    }
}
