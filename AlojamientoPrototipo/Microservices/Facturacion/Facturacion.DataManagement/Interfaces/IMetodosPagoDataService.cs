using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Interfaces;

public interface IMetodosPagoDataService
{
    Task<IEnumerable<MetodoPagoClienteDataModel>> GetAllAsync();
    Task<MetodoPagoClienteDataModel?> GetByExternalIdAsync(Guid externalId);
}
