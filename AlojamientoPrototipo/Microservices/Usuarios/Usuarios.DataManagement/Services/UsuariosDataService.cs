using Usuarios.DataAccess.Repositories.Interfaces;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Mappers;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Services;

public class UsuariosDataService : IUsuariosDataService
{
    private readonly IUsuariosRepository _repo;

    public UsuariosDataService(IUsuariosRepository repo) => _repo = repo;

    public async Task<IEnumerable<UsuarioDataModel>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(UsuariosMapper.ToDataModel);
    }

    public async Task<UsuarioDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? UsuariosMapper.ToDataModel(entity) : null;
    }

    public async Task<UsuarioDataModel?> GetByEmailAsync(string email)
    {
        var entity = await _repo.GetByEmailAsync(email);
        return entity != null ? UsuariosMapper.ToDataModel(entity) : null;
    }

    public async Task<UsuarioDataModel> CreateAsync(UsuarioDataModel model)
    {
        var entity = new DataAccess.Entities.UsuarioEntity
        {
            Rol = model.Rol,
            Email = model.Email,
            PasswordHash = model.PasswordHash,
            NombreCompleto = model.NombreCompleto
        };
        var created = await _repo.AddAsync(entity);
        return UsuariosMapper.ToDataModel(created);
    }

    public async Task UpdateAsync(UsuarioDataModel model)
    {
        var entity = await _repo.GetByIdAsync(model.UsuarioId);
        if (entity == null) throw new KeyNotFoundException($"Usuario {model.UsuarioId} no encontrado");
        UsuariosMapper.UpdateEntity(entity, model);
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Usuario {id} no encontrado");
        await _repo.DeleteAsync(entity);
    }
}
