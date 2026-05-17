using Microservicio.Clientes.Business.DTOs.Usuarios;

namespace Microservicio.Clientes.Business.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioResponse>> GetAllAsync();
    Task<UsuarioResponse?> GetByIdAsync(int id);
    Task<ClienteResponse?> GetClienteByIdAsync(int clienteId);
    Task<ClienteResponse?> GetClienteByCedulaAsync(string cedula);
    Task<IEnumerable<ClienteResponse>> GetAllClientesAsync(int page = 1, int size = 10, string? nombre = null);
    /// <summary>
    /// Usa el SP sp_registrar_cliente para crear Usuario + Cliente atómicamente.
    /// </summary>
    Task RegistrarClienteAsync(RegistrarClienteRequest request);
    Task ActualizarClienteAsync(int clienteId, ActualizarClienteRequest request);
    Task CambiarEstadoClienteAsync(int clienteId, CambiarEstadoRequest request);
    Task EliminarClienteAsync(int clienteId);
}
