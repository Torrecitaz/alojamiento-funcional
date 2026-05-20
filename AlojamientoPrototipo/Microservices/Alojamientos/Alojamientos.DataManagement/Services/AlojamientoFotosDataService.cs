using Alojamientos.DataAccess.Entities;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Mappers;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.DataManagement.Services;

public class AlojamientoFotosDataService : IAlojamientoFotosDataService
{
    private readonly IAlojamientoFotosRepository _repo;

    public AlojamientoFotosDataService(IAlojamientoFotosRepository repo) => _repo = repo;

    public async Task<IEnumerable<AlojamientoFotoDataModel>> GetByAlojamientoIdAsync(int alojamientoId)
    {
        var entities = await _repo.FindAsync(f => f.AlojamientoId == alojamientoId);
        return entities.OrderBy(f => f.Orden).Select(AlojamientosMapper.ToDataModel);
    }

    public async Task<AlojamientoFotoDataModel> CreateAsync(AlojamientoFotoDataModel model)
    {
        var entity = new AlojamientoFotoEntity
        {
            AlojamientoId = model.AlojamientoId,
            Url = model.Url,
            Orden = model.Orden,
            Descripcion = model.Descripcion
        };
        var created = await _repo.AddAsync(entity);
        return AlojamientosMapper.ToDataModel(created);
    }

    public async Task DeleteAsync(int fotoId)
    {
        var entity = await _repo.GetByIdAsync(fotoId);
        if (entity == null) throw new KeyNotFoundException($"Foto {fotoId} no encontrada");
        await _repo.DeleteAsync(entity);
    }
}
