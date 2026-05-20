using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Repositories.Interfaces;

public interface IUsuariosRepository : IRepositoryBase<UsuarioEntity>
{
    Task<UsuarioEntity?> GetByEmailAsync(string email);
}
