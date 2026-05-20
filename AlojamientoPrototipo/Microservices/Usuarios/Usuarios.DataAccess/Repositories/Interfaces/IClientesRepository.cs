using Usuarios.DataAccess.Entities;

namespace Usuarios.DataAccess.Repositories.Interfaces;

public interface IClientesRepository : IRepositoryBase<ClienteEntity>
{
    Task<ClienteEntity?> GetByCedulaAsync(string cedula);
    Task<ClienteEntity?> GetByUsuarioIdAsync(int usuarioId);
    Task RegistrarClienteSPAsync(string email, string password, string nombre, string cedula, string telefono, string domicilio);
}
