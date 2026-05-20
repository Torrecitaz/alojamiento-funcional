using Facturacion.Business.DTOs;
using Facturacion.Business.Interfaces;
using Facturacion.Business.Mappers;
using Facturacion.DataManagement.Interfaces;

namespace Facturacion.Business.Services;

public class MetodosPagoService : IMetodosPagoService
{
    private readonly IMetodosPagoDataService _dataService;

    public MetodosPagoService(IMetodosPagoDataService dataService) => _dataService = dataService;

    public async Task<IEnumerable<MetodoPagoResponse>> GetAllAsync()
    {
        var entities = await _dataService.GetAllAsync();
        return entities.Select(FacturacionBusinessMapper.ToResponse);
    }
}
