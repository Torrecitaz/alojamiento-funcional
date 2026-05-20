using Usuarios.Business.DTOs.Clientes;
using Usuarios.Business.Exceptions;
using Usuarios.Business.Interfaces;
using Usuarios.Business.Mappers;
using Usuarios.DataManagement.Interfaces;

namespace Usuarios.Business.Services;

public class ClientesService : IClientesService
{
    private readonly IClientesDataService _clienteData;
    private readonly IUsuariosDataService _usuarioData;

    public ClientesService(IClientesDataService clienteData, IUsuariosDataService usuarioData)
    {
        _clienteData = clienteData;
        _usuarioData = usuarioData;
    }

    public async Task<IEnumerable<ClienteResponse>> GetAllAsync(int page, int size, string? nombre)
    {
        var models = await _clienteData.GetAllAsync();

        if (!string.IsNullOrEmpty(nombre))
            models = models.Where(c => c.Usuario != null &&
                c.Usuario.NombreCompleto.Contains(nombre, StringComparison.OrdinalIgnoreCase));

        return models.Skip((page - 1) * size).Take(size).Select(ClientesBusinessMapper.ToResponse);
    }

    public async Task<ClienteResponse?> GetByIdAsync(int clienteId)
    {
        var model = await _clienteData.GetByIdAsync(clienteId);
        return model != null ? ClientesBusinessMapper.ToResponse(model) : null;
    }

    public async Task<ClienteResponse?> GetByCedulaAsync(string cedula)
    {
        var model = await _clienteData.GetByCedulaAsync(cedula);
        return model != null ? ClientesBusinessMapper.ToResponse(model) : null;
    }

    public async Task RegistrarClienteAsync(RegistrarClienteRequest request)
    {
        await _clienteData.RegistrarClienteAsync(
            request.Email, request.Password, request.NombreCompleto,
            request.Cedula, request.Telefono, request.Domicilio);
    }

    public async Task ActualizarClienteAsync(int clienteId, ActualizarClienteRequest request)
    {
        var cliente = await _clienteData.GetByIdAsync(clienteId)
            ?? throw new ClienteNotFoundException(clienteId);

        cliente.Telefono = request.Telefono;
        cliente.Domicilio = request.Domicilio;
        if (request.FotoUrl != null) cliente.FotoUrl = request.FotoUrl;

        await _clienteData.UpdateAsync(cliente);

        // Actualizar nombre del usuario asociado
        if (cliente.UsuarioId.HasValue)
        {
            var usuario = await _usuarioData.GetByIdAsync(cliente.UsuarioId.Value);
            if (usuario != null)
            {
                usuario.NombreCompleto = request.NombreCompleto;
                await _usuarioData.UpdateAsync(usuario);
            }
        }
    }

    public async Task CambiarEstadoAsync(int clienteId, CambiarEstadoRequest request)
    {
        var cliente = await _clienteData.GetByIdAsync(clienteId)
            ?? throw new ClienteNotFoundException(clienteId);

        if (cliente.UsuarioId.HasValue)
        {
            var usuario = await _usuarioData.GetByIdAsync(cliente.UsuarioId.Value);
            if (usuario != null)
            {
                usuario.Estado = request.Activo;
                await _usuarioData.UpdateAsync(usuario);
            }
        }
    }

    public async Task EliminarClienteAsync(int clienteId)
    {
        var cliente = await _clienteData.GetByIdAsync(clienteId)
            ?? throw new ClienteNotFoundException(clienteId);

        await _clienteData.DeleteAsync(clienteId);

        if (cliente.UsuarioId.HasValue)
            await _usuarioData.DeleteAsync(cliente.UsuarioId.Value);
    }
}
