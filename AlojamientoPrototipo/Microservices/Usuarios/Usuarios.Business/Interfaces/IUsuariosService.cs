using Usuarios.Business.DTOs.Usuarios;

namespace Usuarios.Business.Interfaces;

public interface IUsuariosService
{
    Task<IEnumerable<UsuarioResponse>> GetAllAsync();
    Task<UsuarioResponse?> GetByIdAsync(int id);
}
