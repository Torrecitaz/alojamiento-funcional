using Microservicio.Cliente.DatAccess.Entities.Usuarios;

namespace Microservicio.Clientes.DataManagement.Interfaces;

public interface IClienteRepository : IRepository<ClienteEntity>
{
    Task<ClienteEntity?> GetByCedulaAsync(string cedula);
    Task<ClienteEntity?> GetByUsuarioIdAsync(int usuarioId);
    /// <summary>
    /// Llama al SP sp_registrar_cliente de la base DB_Usuarios.
    /// Crea el Usuario y el Cliente en una sola transacción atómica.
    /// </summary>
    Task RegistrarClienteSPAsync(string email, string password, string nombre, string cedula, string telefono, string domicilio);
}
