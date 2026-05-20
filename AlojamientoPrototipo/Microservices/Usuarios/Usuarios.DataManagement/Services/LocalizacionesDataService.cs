using Usuarios.DataAccess.Repositories.Interfaces;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Mappers;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Services;

public class LocalizacionesDataService : ILocalizacionesDataService
{
    private readonly ILocalizacionesRepository _repo;

    public LocalizacionesDataService(ILocalizacionesRepository repo) => _repo = repo;

    public async Task<IEnumerable<LocalizacionDataModel>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(LocalizacionesMapper.ToDataModel);
    }

    public async Task<LocalizacionDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? LocalizacionesMapper.ToDataModel(entity) : null;
    }

    public async Task<LocalizacionDataModel> CreateAsync(LocalizacionDataModel model)
    {
        var entity = new DataAccess.Entities.LocalizacionEntity
        {
            Descripcion = model.Descripcion
        };

        if (!string.IsNullOrEmpty(model.PoligonoWkt))
        {
            var reader = new NetTopologySuite.IO.WKTReader();
            var geometry = reader.Read(model.PoligonoWkt);
            if (geometry is NetTopologySuite.Geometries.Polygon polygon)
            {
                entity.Poligono = polygon;
            }
            else
            {
                throw new ArgumentException("El WKT proporcionado no es un polígono válido.");
            }
        }

        var created = await _repo.AddAsync(entity);
        return LocalizacionesMapper.ToDataModel(created);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Localización {id} no encontrada");
        await _repo.DeleteAsync(entity);
    }
}
