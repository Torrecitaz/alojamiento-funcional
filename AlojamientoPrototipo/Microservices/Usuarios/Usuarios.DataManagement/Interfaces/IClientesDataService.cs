using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Interfaces;

public interface IClientesDataService
{
    Task<IEnumerable<ClienteDataModel>> GetAllAsync();
    Task<ClienteDataModel?> GetByIdAsync(int id);
    Task<ClienteDataModel?> GetByCedulaAsync(string cedula);
    Task<ClienteDataModel?> GetByUsuarioIdAsync(int usuarioId);
    Task RegistrarClienteAsync(string email, string password, string nombre, string cedula, string telefono, string domicilio);
    Task UpdateAsync(ClienteDataModel model);
    Task DeleteAsync(int id);
}
