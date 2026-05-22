using Reservas.DataManagement.Models;

namespace Reservas.DataManagement.Interfaces;

public interface IReservasDataService
{
    Task<ReservaDataModel?> GetByIdAsync(int id);
    Task<IEnumerable<ReservaDataModel>> GetByClienteIdAsync(int clienteId);
    Task<ReservaDataModel> CreateAsync(ReservaDataModel model);
    Task UpdateStatusAsync(int id, string nuevoEstado);
    Task DeleteAsync(int id);
    Task<ReservaDataModel?> GetByCodigoAsync(string codigo);
}

public interface IDescuentosDataService
{
    Task<DescuentoDataModel?> GetByCodigoAsync(string codigo);
}
