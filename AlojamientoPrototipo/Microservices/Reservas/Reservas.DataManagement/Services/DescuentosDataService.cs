using Reservas.DataAccess.Repositories.Interfaces;
using Reservas.DataManagement.Interfaces;
using Reservas.DataManagement.Mappers;
using Reservas.DataManagement.Models;

namespace Reservas.DataManagement.Services;

public class DescuentosDataService : IDescuentosDataService
{
    private readonly IDescuentosRepository _repo;

    public DescuentosDataService(IDescuentosRepository repo) => _repo = repo;

    public async Task<DescuentoDataModel?> GetByCodigoAsync(string codigo)
    {
        var entities = await _repo.FindAsync(d => d.Codigo == codigo);
        var entity = entities.FirstOrDefault();
        return entity != null ? ReservasMapper.ToDataModel(entity) : null;
    }
}
