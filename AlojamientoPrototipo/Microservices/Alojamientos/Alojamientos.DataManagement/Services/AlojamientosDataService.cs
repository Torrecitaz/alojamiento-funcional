using Alojamientos.DataAccess.Entities;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Mappers;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.DataManagement.Services;

public class AlojamientosDataService : IAlojamientosDataService
{
    private readonly IAlojamientosRepository _repo;

    public AlojamientosDataService(IAlojamientosRepository repo) => _repo = repo;

    public async Task<IEnumerable<AlojamientoDataModel>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(AlojamientosMapper.ToDataModel);
    }

    public async Task<AlojamientoDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? AlojamientosMapper.ToDataModel(entity) : null;
    }

    public async Task<AlojamientoDataModel> CreateAsync(AlojamientoDataModel model)
    {
        var entity = new AlojamientoEntity
        {
            SocioId = model.SocioId,
            TipoAlojamientoId = model.TipoAlojamientoId,
            Nombre = model.Nombre,
            Ciudad = model.Ciudad,
            Direccion = model.Direccion,
            Descripcion = model.Descripcion,
            Estrellas = model.Estrellas,
            AdmiteMascotas = model.AdmiteMascotas,
            TienePiscina = model.TienePiscina,
            TieneParqueadero = model.TieneParqueadero
        };
        var created = await _repo.AddAsync(entity);
        return AlojamientosMapper.ToDataModel(created);
    }

    public async Task UpdateAsync(AlojamientoDataModel model)
    {
        var entity = await _repo.GetByIdAsync(model.AlojamientoId);
        if (entity == null) throw new KeyNotFoundException($"Alojamiento {model.AlojamientoId} no encontrado");
        
        AlojamientosMapper.UpdateEntity(entity, model);
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Alojamiento {id} no encontrado");
        await _repo.DeleteAsync(entity);
    }
}
