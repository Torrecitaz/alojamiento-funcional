using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Interfaces;

public interface IFacturasDataService
{
    Task<FacturaDataModel?> GetByIdAsync(int id);
    Task<IEnumerable<FacturaDataModel>> GetByReservaIdAsync(int reservaId);
    Task<FacturaDataModel> CreateAsync(FacturaDataModel model);
    Task UpdateStatusAsync(int id, string nuevoEstado);
}
