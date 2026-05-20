using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Interfaces;

public interface IUsuariosDataService
{
    Task<IEnumerable<UsuarioDataModel>> GetAllAsync();
    Task<UsuarioDataModel?> GetByIdAsync(int id);
    Task<UsuarioDataModel?> GetByEmailAsync(string email);
    Task<UsuarioDataModel> CreateAsync(UsuarioDataModel model);
    Task UpdateAsync(UsuarioDataModel model);
    Task DeleteAsync(int id);
}
