using Microservicio.Clientes.Business.DTOs.Usuarios;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IClienteRepository _clienteRepo;

    public UsuarioService(IUsuarioRepository usuarioRepo, IClienteRepository clienteRepo)
    {
        _usuarioRepo = usuarioRepo;
        _clienteRepo = clienteRepo;
    }

    public async Task<IEnumerable<UsuarioResponse>> GetAllAsync()
    {
        var usuarios = await _usuarioRepo.GetAllAsync();
        return usuarios.Select(u => new UsuarioResponse(
            u.UsuarioId, u.Rol, u.Email, u.NombreCompleto, u.Estado, u.FechaCreacion));
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id)
    {
        var u = await _usuarioRepo.GetByIdAsync(id);
        if (u == null) return null;
        return new UsuarioResponse(u.UsuarioId, u.Rol, u.Email, u.NombreCompleto, u.Estado, u.FechaCreacion);
    }

    public async Task<ClienteResponse?> GetClienteByIdAsync(int clienteId)
    {
        var c = await _clienteRepo.GetByIdAsync(clienteId);
        return c == null ? null : MapCliente(c);
    }

    public async Task<ClienteResponse?> GetClienteByCedulaAsync(string cedula)
    {
        var c = await _clienteRepo.GetByCedulaAsync(cedula);
        return c == null ? null : MapCliente(c);
    }

    public async Task<IEnumerable<ClienteResponse>> GetAllClientesAsync(int page = 1, int size = 10, string? nombre = null)
    {
        var clientes = await _clienteRepo.GetAllAsync();

        if (!string.IsNullOrEmpty(nombre))
        {
            clientes = clientes.Where(c => c.Usuario != null && 
                                           c.Usuario.NombreCompleto.Contains(nombre, StringComparison.OrdinalIgnoreCase));
        }

        var paginated = clientes.Skip((page - 1) * size).Take(size);
        return paginated.Select(MapCliente);
    }

    /// <summary>
    /// Registra un nuevo cliente usando el SP sp_registrar_cliente de la DB.
    /// </summary>
    public async Task RegistrarClienteAsync(RegistrarClienteRequest request)
    {
        await _clienteRepo.RegistrarClienteSPAsync(
            request.Email, request.Password, request.NombreCompleto,
            request.Cedula, request.Telefono, request.Domicilio);
    }

    public async Task ActualizarClienteAsync(int clienteId, ActualizarClienteRequest request)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId);
        if (cliente == null) throw new KeyNotFoundException($"No se encontró el cliente {clienteId}");

        if (cliente.Usuario != null)
        {
            cliente.Usuario.NombreCompleto = request.NombreCompleto;
            await _usuarioRepo.UpdateAsync(cliente.Usuario);
        }

        cliente.Telefono = request.Telefono;
        cliente.Domicilio = request.Domicilio;
        if (request.FotoUrl != null)
        {
            cliente.FotoUrl = request.FotoUrl;
        }

        await _clienteRepo.UpdateAsync(cliente);
    }

    public async Task CambiarEstadoClienteAsync(int clienteId, CambiarEstadoRequest request)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId);
        if (cliente == null) throw new KeyNotFoundException($"No se encontró el cliente {clienteId}");

        if (cliente.Usuario != null)
        {
            cliente.Usuario.Estado = request.Activo;
            await _usuarioRepo.UpdateAsync(cliente.Usuario);
        }
    }

    public async Task EliminarClienteAsync(int clienteId)
    {
        var cliente = await _clienteRepo.GetByIdAsync(clienteId);
        if (cliente == null) throw new KeyNotFoundException($"No se encontró el cliente {clienteId}");

        await _clienteRepo.DeleteAsync(cliente);
        
        if (cliente.UsuarioId.HasValue)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(cliente.UsuarioId.Value);
            if (usuario != null)
                await _usuarioRepo.DeleteAsync(usuario);
        }
    }

    private static ClienteResponse MapCliente(Cliente.DatAccess.Entities.Usuarios.ClienteEntity c)
    {
        UsuarioResponse? usuario = c.Usuario != null
            ? new UsuarioResponse(c.Usuario.UsuarioId, c.Usuario.Rol, c.Usuario.Email,
                c.Usuario.NombreCompleto, c.Usuario.Estado, c.Usuario.FechaCreacion)
            : null;

        return new ClienteResponse(
            c.ClienteId, c.UsuarioId, c.Cedula, c.FotoUrl, c.Telefono,
            c.Domicilio, c.Email, c.TotalReservas, c.FechaCreacion, usuario);
    }
}
