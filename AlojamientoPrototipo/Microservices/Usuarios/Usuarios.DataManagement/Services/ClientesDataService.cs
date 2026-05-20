using Usuarios.DataAccess.Repositories.Interfaces;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Mappers;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Services;

public class ClientesDataService : IClientesDataService
{
    private readonly IClientesRepository _repo;

    public ClientesDataService(IClientesRepository repo) => _repo = repo;

    public async Task<IEnumerable<ClienteDataModel>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(ClientesMapper.ToDataModel);
    }

    public async Task<ClienteDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? ClientesMapper.ToDataModel(entity) : null;
    }

    public async Task<ClienteDataModel?> GetByCedulaAsync(string cedula)
    {
        var entity = await _repo.GetByCedulaAsync(cedula);
        return entity != null ? ClientesMapper.ToDataModel(entity) : null;
    }

    public async Task<ClienteDataModel?> GetByUsuarioIdAsync(int usuarioId)
    {
        var entity = await _repo.GetByUsuarioIdAsync(usuarioId);
        return entity != null ? ClientesMapper.ToDataModel(entity) : null;
    }

    public async Task RegistrarClienteAsync(string email, string password, string nombre, string cedula, string telefono, string domicilio)
    {
        await _repo.RegistrarClienteSPAsync(email, password, nombre, cedula, telefono, domicilio);
    }

    public async Task UpdateAsync(ClienteDataModel model)
    {
        var entity = await _repo.GetByIdAsync(model.ClienteId);
        if (entity == null) throw new KeyNotFoundException($"Cliente {model.ClienteId} no encontrado");
        ClientesMapper.UpdateEntity(entity, model);
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Cliente {id} no encontrado");
        await _repo.DeleteAsync(entity);
    }
}
