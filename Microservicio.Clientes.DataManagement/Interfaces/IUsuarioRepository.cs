using Microservicio.Cliente.DatAccess.Entities.Usuarios;

namespace Microservicio.Clientes.DataManagement.Interfaces;

public interface IUsuarioRepository : IRepository<UsuarioEntity>
{
    Task<UsuarioEntity?> GetByEmailAsync(string email);
}
