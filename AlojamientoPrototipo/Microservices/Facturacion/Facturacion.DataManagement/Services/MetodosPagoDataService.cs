using Facturacion.DataAccess.Repositories.Interfaces;
using Facturacion.DataManagement.Interfaces;
using Facturacion.DataManagement.Mappers;
using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Services;

public class MetodosPagoDataService : IMetodosPagoDataService
{
    private readonly IMetodosPagoRepository _repo;

    public MetodosPagoDataService(IMetodosPagoRepository repo) => _repo = repo;

    public async Task<IEnumerable<MetodoPagoClienteDataModel>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(FacturacionMapper.ToDataModel);
    }
}
